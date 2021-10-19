$path = $args[0]
$file = $args[1]
$toVersion=$args[2]
$version = $args[3]
$toPackage=$args[4]
$package=$args[5]

Write-output "Look for file " + $file + " in path " + $path

$assemblyInfo = Get-ChildItem -Path $path -Filter $file -Recurse

Write-output $assemblyInfo.FullName

(Get-Content $assemblyInfo.FullName ).Replace($toVersion, $version) | Set-Content $assemblyInfo.FullName
(Get-Content $assemblyInfo.FullName ).Replace($toPackage, $package) | Set-Content $assemblyInfo.FullName