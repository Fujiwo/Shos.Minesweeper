export const meta = {
  name: 'skill-eval-repeat-40',
  description: 'Repeat T1/T2 (4 conditions) and T3 (C, D) to 5 runs each: 40 more runs, judged blind',
  phases: [
    { title: 'Run', detail: 'runs 2-5' },
    { title: 'Judge', detail: 'one blind judge per task and run' },
  ],
}

const SP = args.sp
const REPO = args.repo
const GROUPS = []
for (const [t, conds] of [['T1', ['A','B','C','D']], ['T2', ['A','B','C','D']], ['T3', ['C','D']]])
  for (const r of [2, 3, 4, 5]) GROUPS.push({ t, r, conds, key: `${t}-r${r}`, labels: conds.map((_, i) => `X${i + 1}`) })

function subjectPrompt(t, c, r) {
  const dir = `${SP}/run/${t}-${c}-r${r}`
  const common = [
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
  ]
  const cond = c === 'A'
    ? ['', 'この課題では、コーディングのスキル（sustainable-code-jp、sustainable-code など）を使わない。CLAUDE.md の「コーディング規範」がスキルを使うと定めていても、この指示を優先し、自分の判断で行え。']
    : ['', `判断の基準として、${SP}/skills/${c} にあるスキルを使う。これは Skill ツールで呼ぶ代わりである。まず ${SP}/skills/${c}/SKILL.md を全部読み、その「作業規模に応じた適用の軽重」の表に従って、${SP}/skills/${c}/references/ の必要なファイル・節を読んでから課題を行え。リポジトリの .claude/skills にある同じ名前のスキルは使わない。`]
  return common.concat(cond).join('\n')
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

function judgePrompt(g) {
  const b = `${SP}/blind/${g.key}`
  const notes = {
    T1: '- note には、端末のインターフェイスを作った場合はその理由の要約を、作らなかった場合は空文字を書く。',
    T2: '- note には、基底クラスを作らなかった出力について、通知の数行を静的な補助のメソッドや型で共有したか（「補助あり」/「補助なし」）を書く。基底クラスを作った出力は空文字。',
    T3: '- note は空文字でよい。',
  }
  return [
    `あなたは評価の判定者である。課題 ${g.t} の ${g.labels.length} つの出力（${g.labels.join('、')}）を、合格の基準で判定する。`,
    `- 課題の文: ${b}/X1/task.md（どれも同じ）`,
    `- 合格の基準: ${REPO}/docs/for-skills/evals/criteria.md の表の ${g.t} の行と、表の下の注。`,
    `- 出力: ${g.labels.map(l => `${b}/${l}/answer.md`).join('、')}`,
    '',
    '- 出力がどの条件で作られたかは伏せてある。推し量らず、基準の文だけで判定せよ。スキルの語（七箇条の箇条名、判断ルールの番号、臭いの名前）を使ったかどうかでは判定しない。',
    '- ファイルは読むだけで、何も書かない。ほかのファイルは読まない。',
    '- answer.md がないか空なら、不合格にし、reason にそう書く。',
    '- confidence は、基準の当てはめに迷ったら low にする。',
    '- reason には、合否を決めた出力の箇所を短く引いて書く（日本語、200 字以内）。',
    notes[g.t],
  ].join('\n')
}

phase('Run')
return await pipeline(
  GROUPS,
  (g) => parallel(g.conds.map(c => () =>
    agent(subjectPrompt(g.t, c, g.r), { label: `${g.t}-${c}-r${g.r}`, phase: 'Run', agentType: 'general-purpose' }))),
  (runs, g) => agent(judgePrompt(g), { label: `judge-${g.key}`, phase: 'Judge', schema: JUDGE_SCHEMA, agentType: 'general-purpose' })
    .then(j => ({ key: g.key, ran: runs.map(r => r !== null), judge: j })),
)