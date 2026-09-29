export const meta = {
  name: 'skill-eval-dprime-10',
  description: "Run T1/T2 five times each under condition D' (S11+S15+S16), judged blind",
  phases: [
    { title: 'Run', detail: 'T1, T2 x 5 under D\'' },
    { title: 'Judge', detail: 'one blind judge per task' },
  ],
}

const SP = args.sp
const REPO = args.repo
const C = 'Dp'
const TASKS = ['T1', 'T2']
const RUNS = [1, 2, 3, 4, 5]

function subjectPrompt(t, r) {
  const dir = `${SP}/run/${t}-${C}-r${r}`
  return [
    `あなたは、評価の課題の被験者である。作業フォルダーは ${dir} である。`,
    `${dir}/task.md を読み、その課題を行え。`,
    '',
    '守ること:',
    `- 作業フォルダーの外のファイルを読まない・書かない。リポジトリ ${REPO} の docs やソースコードも読まない（下にスキルのフォルダーが書かれていれば、それだけは読んでよい）。`,
    '- Skill ツールを使わない。~/.claude と、リポジトリの .claude/skills を読まない。',
    '- ユーザーには質問できない。決まっていないことは仮定を置いて進め、仮定を書く。',
    '- CLAUDE.md の開発手順（工程、承認、レビューや skill-log への記録）は、この課題には当てはまらない。docs には何も書かない。',
    '- ウェブは使わない。',
    `- 課題の成果（設計、レビュー）を、日本語で ${dir}/answer.md に書け。`,
    '- 最後の返答は、answer.md を書いたことを 1 行で述べるだけにする。',
    '',
    `判断の基準として、${SP}/skills/${C} にあるスキルを使う。これは Skill ツールで呼ぶ代わりである。まず ${SP}/skills/${C}/SKILL.md を全部読み、その「作業規模に応じた適用の軽重」の表に従って、${SP}/skills/${C}/references/ の必要なファイル・節を読んでから課題を行え。リポジトリの .claude/skills にある同じ名前のスキルは使わない。`,
  ].join('\n')
}

const JUDGE_SCHEMA = {
  type: 'object',
  properties: {
    results: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          label: { type: 'string' },
          pass: { type: 'boolean' },
          confidence: { type: 'string', enum: ['high', 'low'] },
          reason: { type: 'string' },
          note: { type: 'string' },
        },
        required: ['label', 'pass', 'confidence', 'reason', 'note'],
      },
    },
  },
  required: ['results'],
}

function judgePrompt(t) {
  const b = `${SP}/blind/${t}`
  const labels = RUNS.map(r => `X${r}`)
  const notes = {
    T1: '- note には、端末のインターフェイスを作った場合はその理由の要約を、作らなかった場合は空文字を書く。',
    T2: '- note には、基底クラスを作らなかった出力について、通知の数行を静的な補助のメソッドや型で共有したか（「補助あり」/「補助なし」）を書く。基底クラスを作った出力は空文字。',
  }
  return [
    `あなたは評価の判定者である。課題 ${t} の ${labels.length} つの出力（${labels.join('、')}）を、合格の基準で判定する。`,
    `- 課題の文: ${b}/X1/task.md（どれも同じ）`,
    `- 合格の基準: ${REPO}/docs/for-skills/evals/criteria.md の表の ${t} の行と、表の下の注。`,
    `- 出力: ${labels.map(l => `${b}/${l}/answer.md`).join('、')}`,
    '',
    '- 出力がどの条件で作られたかは伏せてある。推し量らず、基準の文だけで判定せよ。スキルの語（七箇条の箇条名、判断ルールの番号、臭いの名前）を使ったかどうかでは判定しない。',
    '- ファイルは読むだけで、何も書かない。ほかのファイルは読まない。',
    '- answer.md がないか空なら、不合格にし、reason にそう書く。',
    '- confidence は、基準の当てはめに迷ったら low にする。',
    '- reason には、合否を決めた出力の箇所を短く引いて書く（日本語、200 字以内）。',
    notes[t],
  ].join('\n')
}

phase('Run')
return await pipeline(
  TASKS,
  (t) => parallel(RUNS.map(r => () =>
    agent(subjectPrompt(t, r), { label: `${t}-${C}-r${r}`, phase: 'Run', agentType: 'general-purpose' }))),
  (runs, t) => agent(judgePrompt(t), { label: `judge-${t}`, phase: 'Judge', schema: JUDGE_SCHEMA, agentType: 'general-purpose' })
    .then(j => ({ key: t, ran: runs.map(r => r !== null), judge: j })),
)
