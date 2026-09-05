
// https://github.com/PatriceDargenton/DiffLibLLM
// https://www.nuget.org/packages/DiffLibLLM

using DiffLibLLM;
using DiffLibLLM.DependencyInjection;
using DiffLibLLM.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace TextDiffToHtml
{
    internal static class DiffLibLLMAPI
    {
        internal sealed record SemanticSideBySideRenderResult(
            string Html,
            IReadOnlyList<double> SimilarityThresholds,
            bool Cancelled, bool Modified);

        public static string GetMetaData(string model)
        {
            var services = new ServiceCollection();
            services.AddSemanticDiffWithOllama(model: model);
            var provider = services.BuildServiceProvider();
            var engine = provider.GetRequiredService<SemanticDiffEngine>();
            var result = engine.LoadModelMetadata();
            var modelCreatedAt = FormatDate(result?.ModelCreatedAt);
            var modelModifiedAt = FormatDate(result?.ModelModifiedAt);
            var huggingFaceCreatedAt = FormatDate(result?.HuggingFaceCreatedAt);
            var huggingFaceLastModifiedAt = FormatDate(result?.HuggingFaceLastModifiedAt);
            var sb = new StringBuilder();
            sb.AppendLine("<table style='border-collapse: collapse; margin: 8px 0; min-width: 420px; font-family: Segoe UI, sans-serif;'>");
            sb.AppendLine("  <thead>");
            sb.AppendLine("    <tr>");
            sb.AppendLine("      <th style='text-align: left; padding: 8px 12px; background-color: #1f6feb; color: #fff; border: 1px solid #d0d7de;'>Title</th>");
            sb.AppendLine("      <th style='text-align: left; padding: 8px 12px; background-color: #1f6feb; color: #fff; border: 1px solid #d0d7de;'>Value</th>");
            sb.AppendLine("    </tr>");
            sb.AppendLine("  </thead>");
            sb.AppendLine("  <tbody>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Model</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(model)}</td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Ollama link</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'><a href='{Helper.HtmlEncode(result?.ModelUrl)}' target='_blank' rel='noopener noreferrer'>{Helper.HtmlEncode(result?.ModelUrl)}</a></td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Hugging Face link</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'><a href='{Helper.HtmlEncode(result?.HuggingFaceUrl)}' target='_blank' rel='noopener noreferrer'>{Helper.HtmlEncode(result?.HuggingFaceUrl)}</a></td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Architecture</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(result?.Architecture)}</td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Parameters</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(result?.Parameters?.ToString())}</td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Model size (disk)</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(result?.DiskSize)}</td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Context length</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(result?.ContextLength?.ToString())}</td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Dimensions</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(result?.EmbeddingLength?.ToString())}</td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Quantization</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(result?.Quantization)}</td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Distinct token count</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(result?.DistinctTokenCount?.ToString())}</td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Ollama model — last updated on</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(modelModifiedAt)}</td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Hugging Face model — created on</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(huggingFaceCreatedAt)}</td></tr>");
            sb.AppendLine($"    <tr><td style='padding: 6px 12px; border: 1px solid #d0d7de; background-color: #f6f8fa;'>Hugging Face model — last updated on</td><td style='padding: 6px 12px; border: 1px solid #d0d7de;'>{Helper.HtmlEncode(huggingFaceLastModifiedAt)}</td></tr>");
            sb.AppendLine("  </tbody>");
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private static string FormatDate(DateTime? value)
        {
            return value?.ToString("dd/MM/yyyy") ?? string.Empty;
        }

        public static string TestVectorization(string model,
            bool capitalizeFirstChar = false)
        {
            // DiffEngineLLM
            var services = new ServiceCollection();
            services.AddSemanticDiffWithOllama(model);
            var provider = services.BuildServiceProvider();
            var engine = provider.GetRequiredService<SemanticDiffEngine>();

            try
            {
                var timeStart = DateTime.Now;
                var x = engine.VectorizationExamples(model,
                    topK: 6,
                    capitalizeFirstChar: capitalizeFirstChar);
                var timeEnd = DateTime.Now;
                var duration = timeEnd - timeStart;
                var y = SemanticDiffEngine.BuildVectorizationHtml(model, x, duration);
                Debug.WriteLine($"[SemanticDiff][Vectorization] {model}: Time to compute vectorization examples: {duration.TotalSeconds:F1} s");
                return y;
            }
            catch (Exception ex)
            {
                var msg = "Error:\r\n" + ex.Message +
                    "\r\nProbable cause: Ollama is not running."+
                    "\r\nOther possible cause: This Ollama embeddings model is not installed: " + model;
                //Debug.WriteLine(msg);
                msg = Helper.HtmlEncode(msg);
                return Helper.BuildMinimalHtmlDocument($"<pre>{msg}</pre>");
            }
        }

        public static string TextDiffSideBySide(string left, string right, string model,
            int maxChunkLength, float infThreshold,
            double gapPenalty = 0.25,
            bool showIdenticalLines = true, bool monospacedFont = false, 
            DiffLibLLM.HtmlRenderer? renderer = null) 
        {
            var renderResult = RenderTextDiffSideBySide(
                left,
                right,
                model,
                maxChunkLength,
                infThreshold,
                gapPenalty,
                showIdenticalLines,
                monospacedFont,
                renderer);

            return renderResult.Html;
        }

        public static SemanticSideBySideRenderResult RenderTextDiffSideBySide(
            string left,
            string right,
            string model,
            int maxChunkLength,
            float infThreshold,
            double gapPenalty = 0.25,
            bool showIdenticalLines = true,
            bool monospacedFont = false,
            DiffLibLLM.HtmlRenderer? renderer = null)
        {
            // DiffEngineLLM            
            var services = new ServiceCollection();
            if (string.IsNullOrEmpty(model))
                model = EnumHelper.GetDefaultValue<ModelEnum>().ToShortDescription();
            services.AddSemanticDiffWithOllama(model: model);
            var provider = services.BuildServiceProvider();
            var engine = provider.GetRequiredService<SemanticDiffEngine>();

            var options = new SemanticDiffOptions();
            options.MaxChunkLength = maxChunkLength;
            options.AutoSelectChunkLength = true;

            // CandidateChunkLengths defines the exact chunk sizes evaluated by auto-chunking.
            // Here we probe around the UI value (half, same, double) to keep tuning simple and fast.
            // Distinct/OrderBy ensures a clean ascending list passed to the engine.
            options.CandidateChunkLengths = new[]
                {
                    Math.Max(16, maxChunkLength / 2),
                    Math.Max(1, maxChunkLength),
                    Math.Max(24, maxChunkLength * 2)
                }
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
            options.ModifiedSimilarityThreshold = infThreshold;
            options.GapPenalty = gapPenalty;
            var result = engine.Compute(model, left, right, options, renderer: renderer);
            var similarityThresholds = BuildSimilarityThresholdDistribution(result);
            bool cancelled = renderer?.cancel ?? false;
            bool error = renderer?.error ?? false;
            if (error)
            {
                return new SemanticSideBySideRenderResult(
                    Helper.BuildMinimalHtmlDocument(
                        $"<p>Error:</p><pre>{renderer?.errorMessage}</pre>"),
                    similarityThresholds,
                    true, result.Modified);
            }
            if (cancelled)
            {
                return new SemanticSideBySideRenderResult(
                    Helper.BuildMinimalHtmlDocument("<p>Operation canceled by user.</p>"),
                    similarityThresholds,
                    true, result.Modified);
            }

            return new SemanticSideBySideRenderResult(
                BuildSideBySideHtml(result, showIdenticalLines, monospacedFont, gapPenalty),
                similarityThresholds,
                false, result.Modified);
        }

        private static IReadOnlyList<double> BuildSimilarityThresholdDistribution(
            SemanticDiffResult result)
        {
            var lst = result.Chunks
                .Where(chunk => chunk.SourceText != null && chunk.TargetText != null)
                .Select(chunk => Math.Round(
                    Math.Clamp(chunk.Similarity, 0d, 1d),
                    2,
                    MidpointRounding.AwayFromZero))
                .Distinct()
                .OrderBy(value => value)
                .ToArray();

            if (lst.Length == 0) return lst;

            // Add a small margin to the first and last thresholds to ensure that the gradient
            //  color scale covers the full range of similarities.
            var firstThreshold = Math.Round(
                Math.Clamp(lst[0] - 0.01d, 0d, 1d),
                2,
                MidpointRounding.AwayFromZero);
            var lastThreshold = Math.Round(
                Math.Clamp(lst[^1] + 0.01d, 0d, 1d),
                2,
                MidpointRounding.AwayFromZero);

            return lst
                .Append(firstThreshold)
                .Append(lastThreshold)
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
        }

        private static string BuildSideBySideHtml(SemanticDiffResult result,
            bool showIdenticalLines, bool monospacedFont, double gapPenalty)
        {
            var sb = Helper.GetSideBySideStyle(
                new StringBuilder(), colorizeDiff: true, monospacedFont, showSimilarity: true);

            var chunkingMode = result.AutoChunkingEnabled ? "auto" : "manuel";
            sb.AppendLine($"<p><b>Chunk size choosed</b> : {result.SelectedChunkLength} ({chunkingMode})</p>");
            //sb.AppendLine($"<p><b>Gap penalty</b> : {gapPenalty}</p>"); // Experimental

            int leftLine = 0;
            int rightLine = 0;
            foreach (var chunk in result.Chunks)
            {
                var delta = GetDelta(chunk.Operation);
                var simil = chunk.Similarity.ToString("0.00");

                var hasLeft = chunk.SourceText != null;
                var hasRight = chunk.TargetText != null;
                if (hasLeft) leftLine++;
                if (hasRight) rightLine++;

                if (chunk.Operation == SemanticDiffOperationType.Unchanged && !showIdenticalLines)
                {
                    continue;
                }

                var leftNumber = hasLeft ? leftLine.ToString() : string.Empty;
                var rightNumber = hasRight ? rightLine.ToString() : string.Empty;

                var wordSimilarities = chunk.Operation == SemanticDiffOperationType.Modified
                    ? ComputeWordLevelSimilarities(chunk.SourceText, chunk.TargetText, gapPenalty)
                    : null;

                var leftText = HtmlEncodeByOperation(chunk.SourceText, chunk.Operation,
                    isLeft: true, chunk.Similarity, wordSimilarities?.LeftWordSimilarities);
                var rightText = HtmlEncodeByOperation(chunk.TargetText, chunk.Operation,
                    isLeft: false, chunk.Similarity, wordSimilarities?.RightWordSimilarities);

                sb.AppendLine(
                    $"      <tr>" +
                    $"<td>{leftNumber}</td>" +
                    $"<td>{leftText}</td>" +
                    $"<td>{Helper.HtmlEncode(delta)}</td>" +
                    $"<td>{rightText}</td>" +
                    $"<td>{rightNumber}</td>" +
                    $"<td>{simil}</td></tr>");
            }

            sb.AppendLine("  </table>");
            return sb.ToString();
        }

        private static string GetDelta(SemanticDiffOperationType operation)
        {
            return operation switch
            {
                SemanticDiffOperationType.Unchanged => "==",
                SemanticDiffOperationType.Modified => "<>",
                SemanticDiffOperationType.Inserted => ">>",
                SemanticDiffOperationType.Deleted => "<<",
                _ => ""
            };
        }

        private static string HtmlEncodeByOperation(string? text,
            SemanticDiffOperationType operation, bool isLeft, double similarity,
            IReadOnlyList<double>? wordSimilarities)
        {
            var encoded = Helper.HtmlEncode(text);

            if (operation == SemanticDiffOperationType.Modified)
            {
                return BuildWordLevelGradientHtml(text, isLeft, similarity, wordSimilarities);
            }

            if (operation == SemanticDiffOperationType.Inserted ||
                operation == SemanticDiffOperationType.Deleted)
            {
                return $"<span class='diff'>{encoded}</span>";
            }

            return encoded;
        }

        private static string BuildWordLevelGradientHtml(string? text, bool isLeft,
            double similarity, IReadOnlyList<double>? wordSimilarities)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var tokens = Regex.Split(text, @"(\s+)");
            var sb = new StringBuilder();
            int wordIndex = 0;

            foreach (var token in tokens)
            {
                if (string.IsNullOrEmpty(token)) continue;

                if (string.IsNullOrWhiteSpace(token))
                {
                    sb.Append(Helper.HtmlEncode(token));
                    continue;
                }

                var wordSimilarity = similarity;
                if (wordSimilarities != null && wordIndex < wordSimilarities.Count)
                {
                    wordSimilarity = wordSimilarities[wordIndex];
                }
                var color = GetGradientColorBySimilarity(isLeft, wordSimilarity);

                sb.Append($"<span style='background-color: {color};'>");
                sb.Append(Helper.HtmlEncode(token));
                sb.Append("</span>");
                wordIndex++;
            }

            return sb.ToString();
        }

        private static WordSimilarityMap? ComputeWordLevelSimilarities(string? left, string? right,
            double gapPenalty)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right)) return null;

            var leftWords = Regex.Split(left, @"\s+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToArray();
            var rightWords = Regex.Split(right, @"\s+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToArray();

            if (leftWords.Length == 0 || rightWords.Length == 0) return null;

            var wordGapPenalty = -Math.Abs(gapPenalty);
            int n = leftWords.Length;
            int m = rightWords.Length;
            var dp = new double[n + 1, m + 1];
            var move = new byte[n + 1, m + 1]; // 0=diag,1=up,2=left

            for (int i = 1; i <= n; i++)
            {
                dp[i, 0] = dp[i - 1, 0] + wordGapPenalty;
                move[i, 0] = 1;
            }
            for (int j = 1; j <= m; j++)
            {
                dp[0, j] = dp[0, j - 1] + wordGapPenalty;
                move[0, j] = 2;
            }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    var diagonal = dp[i - 1, j - 1] + ComputeWordSimilarity(leftWords[i - 1], rightWords[j - 1]);
                    var up = dp[i - 1, j] + wordGapPenalty;
                    var leftMove = dp[i, j - 1] + wordGapPenalty;

                    if (diagonal >= up && diagonal >= leftMove)
                    {
                        dp[i, j] = diagonal;
                        move[i, j] = 0;
                    }
                    else if (up >= leftMove)
                    {
                        dp[i, j] = up;
                        move[i, j] = 1;
                    }
                    else
                    {
                        dp[i, j] = leftMove;
                        move[i, j] = 2;
                    }
                }
            }

            var leftSims = new double[n];
            var rightSims = new double[m];

            int x = n;
            int y = m;
            while (x > 0 || y > 0)
            {
                if (x > 0 && y > 0 && move[x, y] == 0)
                {
                    var sim = ComputeWordSimilarity(leftWords[x - 1], rightWords[y - 1]);
                    leftSims[x - 1] = sim;
                    rightSims[y - 1] = sim;
                    x--;
                    y--;
                }
                else if (x > 0 && (y == 0 || move[x, y] == 1))
                {
                    leftSims[x - 1] = 0d;
                    x--;
                }
                else
                {
                    rightSims[y - 1] = 0d;
                    y--;
                }
            }

            return new WordSimilarityMap(leftSims, rightSims);
        }

        private static double ComputeWordSimilarity(string leftWord, string rightWord)
        {
            if (string.Equals(leftWord, rightWord, StringComparison.OrdinalIgnoreCase))
                return 1d;

            var source = leftWord.ToLowerInvariant();
            var target = rightWord.ToLowerInvariant();

            int n = source.Length;
            int m = target.Length;
            if (n == 0 && m == 0) return 1d;
            if (n == 0 || m == 0) return 0d;

            var prev = new int[m + 1];
            var curr = new int[m + 1];

            for (int j = 0; j <= m; j++) prev[j] = j;

            for (int i = 1; i <= n; i++)
            {
                curr[0] = i;
                for (int j = 1; j <= m; j++)
                {
                    var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    curr[j] = Math.Min(
                        Math.Min(curr[j - 1] + 1, prev[j] + 1),
                        prev[j - 1] + cost);
                }

                (prev, curr) = (curr, prev);
            }

            var distance = prev[m];
            var maxLength = Math.Max(n, m);
            var similarity = 1d - ((double)distance / maxLength);
            return Math.Clamp(similarity, 0d, 1d);
        }

        private sealed record WordSimilarityMap(
            IReadOnlyList<double> LeftWordSimilarities,
            IReadOnlyList<double> RightWordSimilarities);

        private static string GetGradientColorBySimilarity(bool isLeft, double similarity)
        {
            similarity = Math.Clamp(similarity, 0d, 1d);
            var ratio = 1d - similarity;
            var baseColor = isLeft ? Const.UpdateCharLevelLeft : Const.UpdateCharLevelRight;

            int red = (int)Math.Round(255 + (baseColor.R - 255) * ratio);
            int green = (int)Math.Round(255 + (baseColor.G - 255) * ratio);
            int blue = (int)Math.Round(255 + (baseColor.B - 255) * ratio);

            return $"#{red:X2}{green:X2}{blue:X2}";
        }

        #region "DiffLibLLM Samples 1 & 2"

        // Réglage ok : 60 .80 .90
        public const string DiffLibLLMLeftSample1 =
@"Cette ligne est bien identique
Cette ligne est également identique
Cette ligne a été supprimée
Cette ligne est une autre ligne identique
Voici encore une autre ligne identique
Cette ligne a été modifiée
Cette ligne a également été modifiée
Voici la dernière ligne identique";

        public const string DiffLibLLMRightSample1 =
@"Cette ligne est identique
Cette ligne est également identique
Cette ligne a été supprimée
Cette ligne est une ligne identique
Voici encore une autre ligne identique
Cette ligne n'a plus rien à voir
Cette ligne a également été modifiée
Voici la dernière ligne identique";

        // Réglage ok : 40 .80 .90
        public const string DiffLibLLMLeftSample2 =
@"This line is the same
        This line is also the same
        This line is yet another equal line
        This line has been added
        This is also another equal line
        This line was changed to this
        And then this was added
        And this line was changed to this
        This is the final equal line";

        public const string DiffLibLLMRightSample2 =
@"Cette ligne est identique
Cette ligne est également identique
Cette ligne est encore une ligne identique
Cette ligne a été ajoutée
Ceci est aussi une ligne différente
Cette ligne n'a vraiment rien à voir
Cette ligne a été modifiée pour devenir ceci
Et puis ceci a été ajouté
Et cette ligne a été modifiée pour devenir ceci
Ceci est la dernière ligne identique";

        #endregion

        #region "DiffLibLLM Sample 3 : Mikael Jakson : Billie Jean"

        public const string DiffLibLLMLeftSample3 =
@"Michael Jackson - Billie Jean - 1982

[Verse 1]
She was more like a beauty queen from a movie scene, uh
I said, ‘Don't mind, but what do you mean, I am the one?
Who will dance on the floor in the round?’
She said I am the one
Who will dance on the floor in the round?
She told me her name was Billie Jean as she caused a scene
Then every head turned with eyes that dreamed of bein' the one, uh
Who will dance on the floor in the round?

[Pre-Chorus]
People always told me, ‘Be careful of what you do,’ uh
‘And don't go around breakin' young girls' hearts’ (Hee-hee)
And mother always told me, ‘Be careful of who you love
And be careful of what you do (Oh, oh)
'Cause the lie becomes the truth’ (Oh, oh), hey

[Chorus]
Billie Jean is not my lover, uh
She's just a girl who claims that I am the one (Oh, baby)
But the kid is not my son (Woo)
Uh, she says I am the one (Oh, baby)
But the kid is not my son (Hee-hee-hee; no, no)
(Hee-hee-hee, woo)

[Verse 2]
For forty days and for forty nights, the law was on her side
But who can stand when she's in demand? Her schemes and plans
'Cause we danced on the floor in the round (Hee, uh, uh)
So take my strong advice
Just remember to always think twice (Don't think twice)
Do think twice (A-hoo)
She told my baby we danced 'til three, then she looked at me
Then showed a photo of a baby cryin', his eyes were like mine (Oh no)
'Cause we danced on the floor in the round, baby (Ooh, hee-hee-hee)

[Pre-Chorus]
People always told me, ‘Be careful of what you do,’ uh
‘And don't go around breakin' young girls' hearts’ (Don't break no hearts; hee-hee)
But she came and stood right by me
Just the smell of sweet perfume (Ha-oh)
This happened much too soon (Ha-oh, ha-ooh)
She called me to her room (Ha-oh, hoo), hey

[Chorus]
Billie Jean is not my lover (Woo)
She's just a girl who claims that I am the one, uh
But the kid is not my son, uh
No-no-no, uh, no-no-no, no-no-no (Woo)
Billie Jean is not my lover, uh
She's just a girl who claims that I am the one
But the kid is not my son (No, no)
She says I am the one (Oh, baby)
But the kid is not my son (No, hee-hee-hee)
(Ah-hee-hee-hee)

[Interlude]
Hee, hoo
(Chicka-boom, chicka-boom, chicka-boom, chicka-boom)

[Chorus]
She says I am the one, uh
But the kid is not my son (No-no-no, woo, uh)
Billie Jean is not my lover, uh
She's just a girl who claims that I am the one (You know what you did to me, baby)
But the kid is not my son
No-no-no (No-no-no, ah), no-no-no-no (No-no-no)
She says I am the one (No, baby)
But the kid is not my son (No-no-no-no; woo, uh)

[Outro]
She says I am the one (You know what you did)
She says he is my son (Breakin' my heart, babe)
She says I am the one
Yeah, yeah, Billie Jean is not my lover, uh
Yeah, Billie Jean is not my lover, uh
Yeah, Billie Jean is not my lover, uh (She is just a girl)
Yeah, Billie Jean is not my lover, uh (She is just a girl; don't call me Billie Jean, hoo)
Billie Jean is not my lover, uh (She is just a girl; she's not at the scene)
Billie Jean is not (Hee), aaow, ooh
Yeah, Billie Jean is not...";

        public const string DiffLibLLMRightSample3 =
@"Michael Jackson - Billie Jean - 1982

[Couplet 1]
Elle ressemblait plus à une reine de beauté sortie d'un film, hein ?
J'ai dit : « Ne t'en fais pas, mais qu'est-ce que tu veux dire par ""c'est moi"" ?
Qui dansera sur la piste ronde ? »
Elle a dit : « C'est moi ! »
Qui dansera sur la piste ronde ?
Elle m'a dit qu'elle s'appelait Billie Jean, et elle a fait sensation.
Alors tous les regards se sont tournés vers elle, des yeux qui rêvaient d'être à sa place, hein ?
Qui dansera sur la piste ronde ?

[Pré-refrain]
On m'a toujours dit : « Fais attention à ce que tu fais », hein
« Et ne brise pas le cœur des jeunes filles » (Hi-hi)
Et ma mère m'a toujours dit : « Fais attention à qui tu aimes »
Et fais attention à ce que tu fais (Oh, oh)
« Car le mensonge devient la vérité » (Oh, oh), hey

[Refrain]
Billie Jean n'est pas mon amante, hein
C'est juste une fille qui prétend que je suis l'élu (Oh, bébé)
Mais l'enfant n'est pas mon fils (Woo)
Hein, elle dit que je suis l'élu (Oh, bébé)
Mais l'enfant n'est pas mon fils (Hi-hi-hi ; non, non)
(Hi-hi-hi, woo)
Test : Cette ligne a été ajoutée dans le refrain pour tester la détection de différences.

[Couplet 2]
Pendant quarante jours et quarante nuits, la loi était de son côté
Mais qui peut résister quand elle est recherchée ? Ses manigances et ses plans
Parce qu'on a dansé en rond sur la piste (Hé, euh, euh)
Alors écoute mon conseil
Réfléchis toujours à deux fois (N'y réfléchis pas à deux fois)
Réfléchis-y à deux fois (A-hoo)
Elle a dit à mon chéri qu'on avait dansé jusqu'à trois heures, puis elle m'a regardé
Et elle m'a montré la photo d'un bébé qui pleurait, ses yeux étaient comme les miens (Oh non)
Parce qu'on a dansé en rond sur la piste, chéri (Ooh, hé-hé-hé)

[Pré-refrain]
On m'a toujours dit : « Fais attention à ce que tu fais », euh
« Et ne brise pas le cœur des jeunes filles » (Ne brise pas de cœurs ; hé-hé)
Mais elle est venue et s'est tenue juste à côté de moi
Juste l'odeur de ce doux parfum (Ha-oh)
C'est arrivé bien trop souvent Bientôt (Ha-oh, ha-ooh)
Elle m'a appelé dans sa chambre (Ha-oh, hoo), hey

[Refrain]
Billie Jean n'est pas mon amante (Woo)
C'est juste une fille qui prétend que je suis l'élu, uh
Mais l'enfant n'est pas mon fils, uh
Non-non-non, uh, non-non-non, non-non-non (Woo)
Billie Jean n'est pas mon amante, uh
C'est juste une fille qui prétend que je suis l'élu
Mais l'enfant n'est pas mon fils (Non, non)
Elle dit que je suis l'élu (Oh, bébé)
Mais l'enfant n'est pas mon fils (Non, hi-hi-hi)
(Ah-hi-hi-hi)

[Interlude]
Hee, hoo
(Chicka-boom, chicka-boom, chicka-boom, chicka-boom)

[Refrain]
Elle dit que je suis l'élu, uh
Mais l'enfant est Ce n'est pas mon fils (Non-non-non, woo, uh)
Billie Jean n'est pas ma maîtresse, uh
C'est juste une fille qui prétend que je suis l'élu (Tu sais ce que tu m'as fait, bébé)
Mais ce gamin n'est pas mon fils
Non-non-non (Non-non-non, ah), non-non-non-non (Non-non-non)
Elle dit que je suis l'élu (Non, bébé)
Mais ce gamin n'est pas mon fils (Non-non-non-non; woo, uh)

[Outro]
Elle dit que je suis l'élu (Tu sais ce que tu as fait)
Elle dit que c'est mon fils (Tu me brises le cœur, bébé)
Elle dit que je suis l'élu
Ouais, ouais, Billie Jean n'est pas ma maîtresse, uh
Ouais, Billie Jean n'est pas ma maîtresse, uh
Ouais, Billie Jean n'est pas ma maîtresse, uh (C'est juste une fille)
Ouais, Billie Jean n'est pas ma maîtresse, uh (C'est juste une fille; ne le fais pas) Appelle-moi Billie Jean, hoo)
Billie Jean n'est pas ma copine, euh (C'est juste une fille ; elle n'est pas là)
Billie Jean n'est pas (Hé), aaow, ooh
Ouais, Billie Jean n'est pas...";

        #endregion

        #region "DiffLibLLM Sample 4 : Mikael Jakson : Beat It"

        public const string DiffLibLLMLeftSample4 =
@"Michael Jackson - Beat It - 1982

They told him, ""Don't you ever come around here""
""Don't wanna see your face, you better disappear""
The fire's in their eyes and their words are really clear
So, beat it, just beat it

You better run, you better do what you can
Don't wanna see no blood, don't be a macho man (ooh)
You wanna be tough, better do what you can
So, beat it, but you wanna be bad

Just beat it (beat it), beat it (beat it)
No one wants to be defeated
Showing how funky and strong is your fight
It doesn't matter who's wrong or right
Just beat it (beat it), just beat it (beat it)
Just beat it (beat it), just beat it (beat it) (ooh)

They're out to get you, better leave while you can
Don't wanna be a boy, you wanna be a man
You wanna stay alive, better do what you can
So, beat it, just beat it

You have to show them that you're really not scared (ooh)
You're playin' with your life, this ain't no truth or dare (ooh)
They'll kick you, then they'll beat you, then they'll tell you it's fair
So, beat it, but you wanna be bad

Just beat it (beat it), beat it (beat it)
No one wants to be defeated
Showing how funky and strong is your fight
It doesn't matter who's wrong or right

Just beat it (beat it), beat it (beat it)
No one wants to be defeated
Showing how funky and strong is your fight
It doesn't matter who's wrong or right

Just beat it (beat it, beat it, beat it)
Beat it (beat it, beat it)
Beat it (beat it, beat it)
Beat it (beat it)
Beat it (beat it, beat it)

Beat it (beat it), beat it (beat it)
No one wants to be defeated
Showing how funky and strong is your fight
It doesn't matter who's wrong or right (who's right)

Just beat it (beat it), beat it (beat it)
No one wants to be defeated (no, no)
Showing how funky and strong is your fight
It doesn't matter who's wrong or right

Just beat it (beat it), beat it (beat it)
No one wants to be defeated (oh, no)
Showing how funky and strong is your fight
It doesn't matter who's wrong or right

Just beat it (beat it), beat it (beat it)
No one wants to be defeated
Showing how funky and strong is your fight
It doesn't matter who's wrong or right (who's right)

Just beat it (beat it), beat it (beat it)
No one wants to be defeated";

        public const string DiffLibLLMRightSample4 =
@"Michael Jackson - Beat It - 1982

Ils lui ont dit : « Ne remets jamais les pieds ici »
« On ne veut pas voir ta tête, tu ferais mieux de disparaître »
Le feu brûle dans leurs yeux et leurs paroles sont très claires
Alors, tire-toi, tire-toi tout simplement

Tu ferais mieux de courir, de faire tout ce que tu peux
Pas envie de voir du sang, ne joue pas les durs (ooh)
Tu veux jouer les durs ? Fais tout ce que tu peux
Alors, tire-toi, même si tu veux jouer les mauvais garçons

Tire-toi (tire-toi), tire-toi (tire-toi)
Personne ne veut être vaincu
Montre à quel point ton combat est intense et audacieux
Peu importe qui a tort ou raison
Tire-toi (tire-toi), tire-toi (tire-toi)
Tire-toi (tire-toi), tire-toi (tire-toi) (ooh)

Ils sont à tes trousses, pars tant qu'il en est encore temps
Tu ne veux plus être un gamin, tu veux être un homme
Tu veux rester en vie ? Fais tout ce que tu peux
Alors, tire-toi, tire-toi tout simplement

Tu dois leur montrer que tu n'as pas peur (ooh)
Tu joues avec ta vie, ce n'est pas un jeu d'action ou vérité (ooh)
Ils te donneront des coups de pied, te tabasseront, puis diront que c'est juste
Alors, tire-toi, même si tu veux jouer les mauvais garçons

Tire-toi (tire-toi), tire-toi (tire-toi)
Personne ne veut être vaincu
Montre à quel point ton combat est intense et audacieux
Peu importe qui a tort ou raison

Tire-toi (tire-toi), tire-toi (tire-toi)
Personne ne veut être vaincu
Montre à quel point ton combat est intense et audacieux
Peu importe qui a tort ou raison

Tire-toi (tire-toi, tire-toi, tire-toi)
Tire-toi (tire-toi, tire-toi)
Tire-toi (tire-toi, tire-toi)
Tire-toi (tire-toi)
Tire-toi (tire-toi, tire-toi)

Tire-toi (tire-toi), tire-toi (tire-toi)
Personne ne veut être vaincu
Montre à quel point ton combat est intense et audacieux
Peu importe... Peu importe qui a tort ou raison (qui a raison)
Test : Cette ligne a été ajoutée pour tester la détection de différences.

Fais-le dégager (dégage), fais-le dégager (dégage)
Personne ne veut être vaincu (non, non)
Montre à quel point ton combat est plein de groove et de force
Peu importe qui a tort ou raison

Fais-le dégager (dégage), fais-le dégager (dégage)
Personne ne veut être vaincu (oh, non)
Montre à quel point ton combat est plein de groove et de force
Peu importe qui a tort ou raison

Fais-le dégager (dégage), fais-le dégager (dégage)
Personne ne veut être vaincu
Montre à quel point ton combat est plein de groove et de force
Peu importe qui a tort ou raison (qui a raison)

Fais-le dégager (dégage), fais-le dégager (dégage)
Personne ne veut être vaincu";

        #endregion

        #region "DiffLibLLM Sample 5 : Mikael Jakson : Thriller"

        public const string DiffLibLLMLeftSample5 =
@"Michael Jackson - Thriller - 1982

It's close to midnight
And something evil's lurking in the dark
Under the moonlight
You see a sight that almost stops your heart
You try to scream
But terror takes the sound before you make it
You start to freeze
As horror looks you right between the eyes
You're paralyzed

'Cause this is thriller, thriller night
And no one's gonna save you from the beast about to strike
You know it's thriller, thriller night
You're fighting for your life inside a killer, thriller tonight, yeah

Ooh, ooh
You hear the door slam
And realize there's nowhere left to run
You feel the cold hand
And wonder if you'll ever see the sun
You close your eyes
And hope that this is just imagination
Girl, but all the while
You hear a creature creepin' up behind
You're out of time

'Cause this is thriller, thriller night
There ain't no second chance against the thing with forty eyes, girl
Thriller, thriller night
You're fighting for your life inside a killer, thriller tonight

Night creatures call
And the dead start to walk in their masquerade
There's no escaping the jaws of the alien this time (they're open wide)
This is the end of your life, ooh

They're out to get you
There's demons closin' in on every side
They will possess you
Unless you change that number on your dial
Now is the time
For you and I to cuddle close together, yeah
All through the night
I'll save you from the terror on the screen
I'll make you see

That this is thriller, thriller night
'Cause I can thrill you more than any ghoul would ever dare try
Thriller, thriller night
So let me hold you tight and share a killer, thriller
Chiller, thriller here tonight
'Cause this is thriller, thriller night
Girl, I can thrill you more than any ghoul would ever dare try
Thriller, thriller night
So let me hold you tight and share a killer, thriller, ow

I'm gonna thrill you tonight
Darkness falls across the land
The midnight hour is close at hand
Creatures crawl in search of blood
To terrorize y'all's neighborhood (I'm gonna thrill you tonight)
And whosoever shall be found
Without the soul for getting down
Must stand and face the hounds of hell
And rot inside a corpse's shell

I'm gonna thrill you tonight
Thriller, ooh baby (thriller)
I'm gonna thrill you tonight (thriller night)
Thriller, all night, oh baby
I'm gonna thrill you tonight
Thriller, thriller night (oh baby)
I'm gonna thrill you tonight
Thriller, all night (oh baby)
Thriller night, babe (thriller night, babe), ooh

The foulest stench is in the air
The funk of forty thousand years
And grizzly ghouls from every tomb
Are closing in to seal your doom
And though you fight to stay alive
Your body starts to shiver
For no mere mortal can resist
The evil of the thriller";

        public const string DiffLibLLMRightSample5 =
@"Michael Jackson - Thriller - 1982

Minuit approche
Et une chose maléfique rôde dans l'ombre
Sous le clair de lune
Tu vois une scène qui te glace le sang
Tu essaies de crier
Mais la terreur t'ôte la voix avant même que tu ne puisses émettre un son
Tu te figes sur place
Alors que l'horreur te fixe droit dans les yeux
Tu es paralysé

Car c'est Thriller, la nuit de Thriller
Et personne ne te sauvera de la bête prête à frapper
Tu sais que c'est Thriller, la nuit de Thriller
Tu luttes pour ta survie dans une nuit de Thriller mortel, ouais

Ooh, ooh
Tu entends la porte claquer
Et tu réalises qu'il n'y a plus d'échappatoire
Tu sens une main glacée
Et tu te demandes si tu reverras jamais le soleil
Tu fermes les yeux
En espérant que ce n'est que le fruit de ton imagination
Mais tout ce temps
Tu entends une créature se glisser derrière toi
Le temps presse

Car c'est Thriller, la nuit de Thriller
Il n'y a pas de seconde chance face à la chose aux quarante yeux
Thriller, la nuit de Thriller
Tu luttes pour ta survie dans une nuit de Thriller mortel

Les créatures de la nuit appellent
Et les morts commencent leur marche macabre
Impossible d'échapper aux mâchoires de la créature cette fois (elles sont grandes ouvertes)
C'est la fin de ta vie, ooh
Test : Cette ligne a été ajoutée pour tester la détection de différences.

Ils sont à tes trousses
Des démons t'encerclent de toutes parts
Ils vont te posséder
À moins que tu ne changes de chaîne
Il est temps
Que nous nous serrions l'un contre l'autre, ouais
Tout au long de la nuit
Je te sauverai de la terreur qui s'affiche à l'écran
Je te ferai comprendre

Que c'est Thriller, la nuit de Thriller
Car je peux te faire frissonner bien plus qu'aucun goule n'oserait le tenter
Thriller, la nuit de Thriller
Alors laisse-moi te serrer fort et partager ce Thriller mortel
Un Thriller frissonnant ce soir
Car c'est Thriller, la nuit de Thriller
Je peux te faire frissonner bien plus qu'aucun goule n'oserait le tenter
Thriller, la nuit de Thriller
Alors laisse-moi te serrer fort et partager ce Thriller mortel

Je vais te faire frissonner ce soir
L'obscurité s'abat sur la terre
Minuit approche Des créatures rampent en quête de sang
Pour terroriser votre quartier (je vais vous faire frissonner ce soir)
Et quiconque se retrouvera
Sans l'âme pour entrer dans la danse
Devra affronter les chiens de l'enfer
Et pourrir dans une carcasse de cadavre

Je vais vous faire frissonner ce soir
Thriller, oh bébé (Thriller)
Je vais vous faire frissonner ce soir (nuit de Thriller)
Thriller, toute la nuit, oh bébé
Je vais vous faire frissonner ce soir
Thriller, nuit de Thriller (oh bébé)
Je vais vous faire frissonner ce soir
Thriller, toute la nuit (oh bébé)
Nuit de Thriller, bébé (nuit de Thriller, bébé), ooh

Une odeur infecte flotte dans l'air
La puanteur de quarante mille ans
Et d'effroyables goules surgies de chaque tombe
Se rapprochent pour sceller votre sort
Et bien que vous luttiez pour survivre
Votre corps se met à trembler
Car nul simple mortel ne peut résister
Au maléfice du Thriller";

        #endregion
                
    }
}
