
#r "paket: 
nuget Fake.Core.Process
nuget Fake.IO.FileSystem
nuget Fake.Core.Target //"
#load ".fake/build.fsx/intellisense.fsx"

open System
open System.IO
//open Fake.DotNet
open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators
//open Fake.Tools

let libPath = "./src"
let testsPath = "./test"

let platformTool tool winTool =
  let tool = if Environment.isUnix then tool else winTool
  tool
  |> ProcessUtils.tryFindFileOnPath
  |> function Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node.exe"
let npmTool = platformTool "npm" "npm.cmd"

let dotnetCli = "dotnet"

let run cmd args workingDir =
  let result =
    Process.execSimple (fun info -> {
      info with
        FileName = cmd
        WorkingDirectory = workingDir
        Arguments = args
    }) TimeSpan.MaxValue
  if result <> 0 then failwithf "'%s %s' failed" cmd args

let delete file = 
    if File.Exists(file) 
    then File.delete file
    else () 

let cleanBundles() = 
    Path.Combine("public", "bundle.js") 
        |> Path.GetFullPath 
        |> delete
    Path.Combine("public", "bundle.js.map") 
        |> Path.GetFullPath
        |> delete 

let cleanCacheDirs() = 
    [ testsPath </> "bin" 
      testsPath </> "obj" 
      libPath </> "bin"
      libPath </> "obj" ]
    |> Shell.cleanDirs

Target.create "Clean" <| fun _ ->
    cleanCacheDirs()
    cleanBundles()

Target.create "InstallNpmPackages" (fun _ ->
  printfn "Node version:"
  run nodeTool "--version" __SOURCE_DIRECTORY__
  run npmTool "--version" __SOURCE_DIRECTORY__
  run npmTool "install" __SOURCE_DIRECTORY__
)

Target.create "RestoreFableTestProject" <| fun _ ->
  run dotnetCli "restore" testsPath

Target.create "RunLiveTests" <| fun _ ->
    run npmTool "run start" testsPath

let publish projectPath = fun t ->
    [ projectPath </> "bin"
      projectPath </> "obj" ] |> Shell.cleanDirs
    run dotnetCli "restore --no-cache" projectPath
    run dotnetCli "pack -c Release" projectPath
    let nugetKey =
        match Environment.environVarOrNone "NUGET_KEY" with
        | Some nugetKey -> nugetKey
        | None -> failwith "The Nuget API key must be set in a NUGET_KEY environmental variable"
    let nupkg = 
        Directory.GetFiles(projectPath </> "bin" </> "Release") 
        |> Seq.head 
        |> Path.GetFullPath

    let pushCmd = sprintf "nuget push %s -s nuget.org -k %s" nupkg nugetKey
    run dotnetCli pushCmd projectPath

Target.create "PublishNuget" (publish libPath)

Target.create "CompileFableTestProject" <| fun _ ->
    run npmTool "run build" testsPath

Target.create "RunTests" <| fun _ ->
    printfn "Building %s with Fable" testsPath
    printfn "Using QUnit cli to run the tests"
    run npmTool "run test" "."
    cleanBundles()

"Clean"
  ==> "InstallNpmPackages"
  ==> "RestoreFableTestProject"
  ==> "RunLiveTests"


"Clean"
 ==> "InstallNpmPackages"
 ==> "RestoreFableTestProject"
 ==> "CompileFableTestProject"
 ==> "RunTests"

Target.runOrDefault "RunTests"