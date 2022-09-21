$resultDirectory = "\TestResults"
$reportsDirectory = "\TestResults\TestReports"
$tests = ".\Eede.Application.Tests", ".\Eede.Domain.Tests", "./Eede.Infrastructure.Tests"

foreach ($i in $tests) {
	if (Test-Path $i$resultDirectory) { Remove-Item $i$resultDirectory -Recurse }
	if (Test-Path $i$reportsDirectory) { Remove-Item $i$reportsDirectory -Recurse }

	dotnet test $i --collect:"XPlat Code Coverage"

	$xmlFileName = (Get-ChildItem $i$resultDirectory -Filter *.xml -Recurse -File)[0].FullName

	reportgenerator -reports:$xmlFileName -targetdir:$i$reportsDirectory -reporttypes:Html
}