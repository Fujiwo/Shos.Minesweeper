using System.Text;

namespace Shos.Minesweeper.Presentation;

/// <summary>
/// 利用者が入力した文字列を、解釈する前に整える（ユーザーの指示、2026-09-27）。どの UI の入力も、この 1 か所を通す。
/// 前後の空白（全角の空白を含む）を除き、Unicode の互換正規化（NFKC）で、全角の数字や記号を半角に、半角のカタカナを全角にそろえる。
/// </summary>
public static class InputText
{
    public static string Normalize(string text) => text.Trim().Normalize(NormalizationForm.FormKC);
}
