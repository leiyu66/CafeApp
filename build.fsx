#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.NpmHelper
let buildDir = "./build"
let testDir = "./tests"
let nunitRunnerPath = "packages/NUnit.Runners/tools/"
let clientDir = "./client"
let clientAssetDir = clientDir @@ "public"
let assetBuildDir = buildDir @@ "public"
Target "Clean" (fun _ -> CleanDirs [buildDir; testDir])
Target "BuildApp" (fun _ ->
          !! "src/**/*.fsproj"
            -- "src/**/*.Tests.fsproj"
            |> MSBuildRelease buildDir "Build"
            |> Log "AppBuild-Output: "
)
Target "BuildTests" (fun _ ->
          !! "src/**/*.Tests.fsproj"
          |> MSBuildDebug testDir "Build"
          |> Log "BuildTests-Output: "
)
Target "RunUnitTests" (fun _ ->
          !! (testDir + "/*.Tests.dll")
          |> NUnit (fun p ->
                      {p with ToolPath = nunitRunnerPath})
)
Target "Client" (fun _ ->
     let npmFilePath = environVarOrDefault "NPM_FILE_PATH" defaultNpmParams.NpmFilePath
     Npm (fun p -> { p with
                      Command = (Run "build")
                      WorkingDirectory = clientDir
                      NpmFilePath = npmFilePath
                   })
     CopyRecursive clientAssetDir assetBuildDir true |> ignore
)
"Clean"
  ==> "BuildApp"
  ==> "BuildTests"
  ==> "RunUnitTests"
RunTargetOrDefault "RunUnitTests"
