#--------------------------------------------------------------
# quick and dirty script to create project files for different 
# .NET versions
#--------------------------------------------------------------
#
# use 'Set-ExecutionPolicy Unrestricted -Force' to enable scripts

$frameworks = "4.5.1"
$namespaces = "RedFoxMQ", "RedFoxMQ.Tests", "RedFoxMQ.Serialization.ProtoBuf", "RedFoxMQ.Serialization.ProtoBuf.Tests"

ForEach ($framework in $frameworks)
{
	Write-Host "Creating projects for: .NET Framework $framework"

	$src = "..\RedFoxMQ.sln"
	$dst = "..\RedFoxMQ (.NET $framework).sln"
	cpi $src $dst -force

	ForEach ($namespace in $namespaces)
	{
		(Get-Content $dst) | Foreach-Object {$_ -replace """$namespace""", """$namespace (.NET $framework)"""} | Set-Content $dst
		(Get-Content $dst) | Foreach-Object {$_ -replace "$namespace.csproj""", "$namespace (.NET $framework).csproj"""} | Set-Content $dst
	}
	
	ForEach ($namespace in $namespaces)
	{
		$src = "..\$namespace\$namespace.csproj"
		$dst = "..\$namespace\$namespace (.NET $framework).csproj"
		cpi $src $dst -force
		
		$ns = @{msb = 'http://schemas.microsoft.com/developer/msbuild/2003'}
		$xml = [xml](gc $dst)
		$xml | Select-Xml "//msb:TargetFrameworkVersion" -Namespace $ns | Foreach {$_.Node.set_InnerText("v$framework")}
		
		$xml | Select-Xml "//msb:OutputPath[text() = '`$(BaseOutputPath)\`$(Configuration)\']" -Namespace $ns | Foreach {$_.Node.set_InnerText("`$(BaseOutputPath)\`$(Configuration)_v$framework\")}
		$xml | Select-Xml "//msb:OutputPath[text() = '`$(BaseOutputPath)\`$(Configuration)\']" -Namespace $ns | Foreach {$_.Node.set_InnerText("`$(BaseOutputPath)\`$(Configuration)_v$framework\")}
		
		ForEach ($project in $namespaces) 
		{
			$xml | Select-Xml "//msb:ProjectReference[@Include = '..\$project\$project.csproj']" -Namespace $ns | Foreach {$_.Node.Include = "..\$project\$project %28.NET $framework%29.csproj"}
			$xml | Select-Xml "//msb:Name[text() = '$project']" -Namespace $ns | Foreach {$_.Node.set_InnerText("$project %28.NET $framework%29")}
		}
		
		$xml.Save($dst)
	}
}
