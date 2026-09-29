export const meta = {
  name: 'skill-eval-first-32',
  description: 'Run 8 skill-evaluation tasks under 4 skill conditions (32 runs) and judge them blind',
  phases: [
    { title: 'Run', detail: '8 tasks x 4 conditions, subagents without conversation context' },
    { title: 'Judge', detail: 'one blind judge per task' },
  ],
}

const SP = args.sp
const REPO = args.repo
const TASKS = ['T1','T2','T3','T4','T5','T6','T7','T8']
const CONDS = ['A','B','C','D']

function subjectPrompt(t, c) {
  const dir = `${SP}/run/${t}-${c}`
  const common = [
    `あなたは、評価の課題の被験者である。作業フォルダーは ${dir} である。`,
    `${dir}/task.md を読み、その課題を行え。`,
    '',
    '守ること:',
    `- 作業フォルダーの外のファイルを読まない・書かない。リポジトリ ${REPO} の docs やソースコードも読まない（下にスキルのフォルダーが書かれていれば、それだけは読んでよい）。`,
    '- Skill ツールを使わない。~/.claude と、リポジトリの .claude/skills を読まない。',
    '- ユーザーには質問できない。決まっていないことは仮定を置いて進め、仮定を書く。',
    '- CLAUDE.md の開発手順（工程、承認、レビューや skill-log への記録）は、この課題には当てはまらない。docs には何も書かない。',
    t === 'T8' ? '- ウェブで調べてよい（WebSearch、WebFetch）。' : '- ウェブは使わない。',
    `- 課題の成果（設計、レビュー、コード、報告）を、日本語で ${dir}/answer.md に書け。T6 のように作業フォルダーにコードを書く課題でも、報告を answer.md に書く。`,
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

function judgePrompt(t) {
  const b = `${SP}/blind/${t}`
  return [
    `あなたは評価の判定者である。課題 ${t} の 4 つの出力（X1〜X4）を、合格の基準で判定する。`,
    `- 課題の文: ${b}/X1/task.md（4 つとも同じ）`,
    `- 合格の基準: ${REPO}/docs/for-skills/evals/criteria.md の表の ${t} の行と、表の下の注。`,
    `- 出力: ${b}/X1/answer.md、${b}/X2/answer.md、${b}/X3/answer.md、${b}/X4/answer.md` + (t === 'T6' ? `。T6 は、各フォルダーの test-log.txt だけで合否を決める（answer.md は参考）。作業フォルダーの .py も見てよい` : ''),
    '',
    '- 出力がどの条件で作られたかは伏せてある。推し量らず、基準の文だけで判定せよ。スキルの語（七箇条の箇条名、判断ルールの番号、臭いの名前）を使ったかどうかでは判定しない。',
    '- ファイルは読むだけで、何も書かない。ほかのファイルは読まない。',
    '- answer.md がないか空なら、不合格にし、reason にそう書く。',
    '- confidence は、基準の当てはめに迷ったら low にする。',
    '- reason には、合否を決めた出力の箇所を短く引いて書く（日本語、200 字以内）。',
    t === 'T4' ? '- note には、その出力が、捨てた Task の例外の指摘を七箇条の箇条名に当てはめたか（当てはめた / 当てはめず正しさなどとして書いた / 指摘なし）を書く。' : '- note は空文字でよい。',
  ].join('\n')
}

phase('Run')
const results = await pipeline(
  TASKS,
  (t) => parallel(CONDS.map(c => () =>
    agent(subjectPrompt(t, c), { label: `${t}-${c}`, phase: 'Run', agentType: 'general-purpose' }))),
  (runs, t) => agent(judgePrompt(t), { label: `judge-${t}`, phase: 'Judge', schema: JUDGE_SCHEMA, agentType: 'general-purpose' })
    .then(j => ({ task: t, ran: runs.map(r => r !== null), judge: j })),
)
return results