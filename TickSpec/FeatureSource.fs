namespace TickSpec

type FeatureSource =
    { 
        Name: string; 
        Scenarios: ScenarioSource [] 
    }
and ScenarioSource =
    { 
        Name: string; 
        Tags: string[];
        Steps: StepSource []; 
        Parameters: (string * string) [] 
    }
    with
    override this.ToString() = this.Name
and StepSource = StepType * LineSource
and StepType =
    | GivenStep of string
    | WhenStep of string
    | ThenStep of string
and LineSource =
    {       
        Number : int
        Text : string
        Bullets : string[] option
        Table : Table option
        Doc : string option
    }
and [<System.Diagnostics.DebuggerStepThrough>]
    Table (header:string[],rows:string[][],tags:string[]) =    
    new (header, rows) = Table(header,rows,[||])
    new (header) = Table(header,[|[||]|],[||])
    new () = Table([||]) 
    member table.Header = header
    member table.Rows = rows
    member table.Tags = tags
    member table.Raw = [|yield header;yield! rows|] 