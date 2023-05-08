# If you want to test it on an unmodified Windows 11, issue this statement first:
#   Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process
# This will revert to default "Security" once the PowerShell session has been closed.
$path = $args[0]
$packageIdPrefix = $args[1]

Write-output "Setting the PackageId in path $path"

$files = Get-ChildItem -Path $path -Recurse -Filter *.csproj

foreach ($file in $files) {
    $xml = [xml](Get-Content $file.FullName)

    # We don't need the explicit namespace support, but we keep it in the comments.
    # $projectNamespace = "http://schemas.microsoft.com/developer/msbuild/2003"
    # $ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    # $ns.AddNamespace("p", $projectNamespace)

    $packageIdNode = $xml.SelectSingleNode("Project/PropertyGroup/PackageId") # ,ns)

    # It's good practice to have a PackageId, but in case we don't, we need to add one preferably in the correct PropertyGroup.
    # To identify this PropertyGroup, we assume that the good one contains a PackageProjectUrl. If that isn't the case, we can't add anything
    if ($packageIdNode -eq $null) {
        $packageProjectUrlNode = $xml.SelectSingleNode("Project/PropertyGroup/PackageProjectUrl") # ,ns)
        if ($packageProjectUrlNode -ne $null) {
            $propertyGroupNode = $packageProjectUrlNode.ParentNode
            $packageIdNode = $xml.CreateElement("PackageId") # , $projectNamespace)
            [void]$propertyGroupNode.AppendChild($packageIdNode)
            $packageId = $file.BaseName
        }
    }
    else {
        $packageId = $packageIdNode.InnerText
    }

    if ($packageIdNode -eq $null) {
        Write-output "$($file.FullName) does not contain a PropertyGroup with a PackageId nor a PackageProjectUrl"
    }
    else {
        $packageId = $packageIdPrefix + $packageId

        $packageIdNode.InnerText = $packageId
        Write-output "$($file.FullName) has NuGet PackageId $packageId"
        [void]$xml.Save($file.FullName)
    }
}
