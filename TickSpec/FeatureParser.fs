module TickSpec.FeatureParser

open System.Text.RegularExpressions
open TickSpec.LineParser
open TickSpec.BlockParser

/// Computes combinations of table values
let internal computeCombinations (tables:Table []) =
    let rec combinations source =
        match source with
        | [] -> [[]]
        | (header, rows) :: xs ->
            [ for row in rows do
                for combinedRow in combinations xs ->
                    (header, row) :: combinedRow ]
    
    let processRow rowSet =
        rowSet
        |> List.fold (fun state (header, rowData) ->
            match state with
            | None -> None
            | Some s ->
                rowData
                |> Map.fold (fun current c v -> 
                    match current with
                    | None -> None
                    | Some m -> 
                        let existingValue = m |> Map.tryFind c
                        match existingValue with
                        | None -> Some (m.Add (c, v))
                        | Some v -> Some m
                        | _ -> None 
                ) (Some s)
        ) (Some Map.empty)

    tables
    |> Seq.map 
        ((fun table -> table.Header, table.Rows) >>
        (fun (header, rows) -> 
            header |> Array.toList |> List.sort,
            rows
            |> Seq.map (fun row ->
                row
                |> Array.mapi (fun i col -> header.[i], col) |> Map.ofArray)
        ))
    // Union tables with the same columns
    |> Seq.groupBy (fun (header, _) -> header)
    |> Seq.map (fun (header, tables) ->
        header,
        Seq.collect (fun (_, rows) -> rows) tables)
    |> Seq.toList
    // Cross-join tables with different columns
    |> combinations
    |> List.map processRow
    |> List.choose id
    |> List.distinct
    |> List.map Map.toList

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