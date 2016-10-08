#r "packages/FAKE/tools/FakeLib.dll"
open Fake
let buildDir = "./build"
let testDir = "./tests"
let nunitRunnerPath = "packages/NUnit.Runners/tools/"
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
"Clean"
  ==> "BuildApp"
  ==> "BuildTests"
  ==> "RunUnitTests"
RunTargetOrDefault "RunUnitTests"
