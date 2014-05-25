pvc.Task("nuget-push", () => {
	pvc.Source("src/Pvc.S3.csproj")
	   .Pipe(new PvcNuGetPack(
		createSymbolsPackage: true
	   ))
	   .Pipe(new PvcNuGetPush());
});
