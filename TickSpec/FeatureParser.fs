module TickSpec.FeatureParser

open System.Text.RegularExpressions
open TickSpec.LineParser
open TickSpec.BlockParser

/// Computes combinations of table values
let internal computeCombinations (tables:Table []):((string*string) list list) =
    // TODO: This method needs to be rewritten
    // The tables should have also header outside, so it will
    // be easy to decide which tables to join, which ones to union.
    // The current implementation does the join. It is also needed
    // to add the ability to union two tables. The join needs also to
    // support the join over more than one column.
    
    let addCellToExampleRow row (header,value) =
        let found =
            row
            |> List.tryFind (fun (h,_) -> header=h)

        match found with
        | Some (_,v) ->
            if v = value then row
            else row
        | None -> (header,value) :: row

    let mergeExampleRows row1 row2 =
        row1
        |> List.fold addCellToExampleRow row2

    /// Computes all combinations
    let rec combinations = function
    | [] -> [[]]
    | table :: tss ->
        [ for row1 in table do
            for row2 in combinations tss ->
                mergeExampleRows row1 row2 ]

    let values = 
        tables
        |> Seq.map (fun table ->
            table.Rows 
            |> Array.map (fun row ->
                row
                |> Array.mapi (fun i col ->
                    table.Header.[i],col)
                |> Array.toList)
            |> Array.toList
        )
        |> Seq.toList

    values |> combinations

/// Replace line with specified named values
let internal replaceLine (xs:seq<string * string>) (scenario,n,tags,line,step) =
    let replace s =
        let lookup (m:Match) =
            let x = m.Value.TrimStart([|'<'|]).TrimEnd([|'>'|])
            xs |> Seq.tryFind (fun (k,_) -> k = x)
            |> (function Some(_,v) -> v | None -> m.Value)
        let pattern = "<([^<]*)>"
        Regex.Replace(s, pattern, lookup)
    let step = 
        match step with
        | GivenStep s -> replace s |> GivenStep
        | WhenStep s -> replace s |> WhenStep
        | ThenStep s  -> replace s |> ThenStep
    let table =
        line.Table 
        |> Option.map (fun table ->
            Table(table.Header,
                table.Rows |> Array.map (fun row ->
                    row |> Array.map (fun col -> replace col)
                )
            )
        )
    let bullets =
        line.Bullets
        |> Option.map (fun bullets -> bullets |> Array.map replace)
    (scenario,n,tags,{line with Table=table;Bullets=bullets},step)

/// Appends shared examples to scenarios as examples
let internal appendSharedExamples (sharedExamples:Table[]) scenarios  =
    if Seq.length sharedExamples = 0 then
        scenarios
    else
        scenarios |> Seq.map (function 
            | scenarioName,tags,steps,None ->
                scenarioName,tags,steps,Some(sharedExamples)
            | scenarioName,tags,steps,Some(exampleTables) ->
                scenarioName,tags,steps,Some(Array.append exampleTables sharedExamples)
        )
          
/// Parses lines of feature
let parseFeature (lines:string[]) =
    let toStep (_,_,_,line,step) = step,line
    let featureName,background,scenarios,sharedExamples = parseBlocks lines     
    let scenarios =
        scenarios 
        |> appendSharedExamples sharedExamples
        |> Seq.collect (function
            | name,tags,steps,None ->
                let steps = 
                    Seq.append background steps
                    |> Seq.map toStep 
                    |> Seq.toArray
                Seq.singleton
                    { Name=name; Tags=tags; Steps=steps; Parameters=[||] }
            | name,tags,steps,Some(exampleTables) ->
                /// All combinations of tables
                let combinations = computeCombinations exampleTables
                // Execute each combination
                combinations |> Seq.mapi (fun i combination ->
                    let name = sprintf "%s(%d)" name i
                    let combination = combination |> Seq.toArray
                    let steps =
                        Seq.append background steps
                        |> Seq.map (replaceLine combination)
                        |> Seq.map toStep
                        |> Seq.toArray
                    { Name=name; Tags=tags; Steps=steps; Parameters=combination }
                )
        )
    { Name=featureName; Scenarios=scenarios |> Seq.toArray }