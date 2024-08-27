open System
open System.IO
open System.Security.Cryptography

// Function to calculate the MD5 hash of a file
let getFileHash (filePath: string) =
    use md5 = MD5.Create()
    use stream = File.OpenRead(filePath)
    let hash = md5.ComputeHash(stream)
    BitConverter.ToString(hash).Replace("-", "").ToLower()

// Function to process files and copy non-duplicates to the target folder
let processFiles (sourceDir: string) (targetDir: string) =
    let mutable fileCount = 1
    let fileHashes = System.Collections.Generic.Dictionary<string, string>()

    let rec processDirectory (dir: string) =
        for file in Directory.GetFiles(dir) do
            let hash = getFileHash file
            if not (fileHashes.ContainsKey(hash)) then
                fileHashes.Add(hash, file)
                let fileName = Path.GetFileName(file)
                let newFileName = sprintf "%02d-%s" fileCount fileName
                let destinationPath = Path.Combine(targetDir, newFileName)
                File.Copy(file, destinationPath)
                printfn "Copied: %s -> %s" file destinationPath
                fileCount <- fileCount + 1
            else
                printfn "Duplicate found: %s" file

        for subDir in Directory.GetDirectories(dir) do
            processDirectory subDir

    if not (Directory.Exists(targetDir)) then
        Directory.CreateDirectory(targetDir) |> ignore

    processDirectory sourceDir

// Main entry point of the console app
[<EntryPoint>]
let main argv =
    if argv.Length <> 2 then
        printfn "Usage: <source directory> <target directory>"
        1 // Return an error code
    else
        let sourceDir = argv.[0]
        let targetDir = argv.[1]

        if Directory.Exists(sourceDir) then
            processFiles sourceDir targetDir
            printfn "Processing completed."
            0 // Return success code
        else
            printfn "Source directory does not exist."
            1 // Return an error code
