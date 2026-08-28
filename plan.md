1. Modify `Eede.Domain/ImageEditing/Blending/DirectImageBlender.cs`
   - Use `replace_with_git_merge_diff` to replace the inner pixel-by-pixel `for (int x = startX; x < maxX; x++)` copying loop in `DirectImageBlender`.
   - Calculate the length of the span to copy: `int length = (maxX - startX) * 4;`
   - Use `fromSpan.Slice(fromPos, length).CopyTo(new Span<byte>(toPixels).Slice(toPos, length));` or `toPixels.AsSpan(toPos, length)` to replace the inner `for` loop.
2. Run Benchmark to verify performance improvement
   - Run `dotnet run --project PerfBench -c Release --filter *DirectImageBlenderBenchmark*`
   - Record the new benchmark results.
3. Complete pre-commit steps to ensure proper testing, verification, review, and reflection are done.
   - Use `pre_commit_instructions` and follow the steps.
   - Run tests: `export PATH="$PATH:$HOME/.dotnet/tools"; find . -name "*.Tests.dll" | grep -v "obj" | while read -r dll; do xvfb-run -a nunit "$dll"; done` (as per memory).
4. Present - Share Speed Boost
   - Create a PR using a PR creation tool (after checking what tools are available if needed or just submitting) or use `submit` tool with title "⚡ Optimize DirectImageBlender using Span.CopyTo" and a description showing the benchmark results (baseline 5.212 ms -> new time).
