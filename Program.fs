// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Flips
open Flips.SliceMap
open Flips.Types

let oils = ["VEG1"; "VEG2"; "OIL1"; "OIL2"; "OIL3" ]

let months = ["January"; "February"; "March"; "April"; "May"; "June" ]

let prices = SMap2.ofList [
    ("January", "VEG1"), 110.0; ("January", "VEG2"), 120.0; ("January", "OIL1"), 130.0; ("January", "OIL2"), 110.0; ("January", "OIL3"), 115.0;
    ("February", "VEG1"), 130.0; ("February", "VEG2"), 130.0; ("February", "OIL1"), 110.0; ("February", "OIL2"), 90.0; ("February", "OIL3"), 115.0;
    ("March", "VEG1"), 110.0; ("March", "VEG2"), 140.0; ("March", "OIL1"), 130.0; ("March", "OIL2"), 100.0; ("March", "OIL3"), 95.0;
    ("April", "VEG1"), 120.0; ("April", "VEG2"), 110.0; ("April", "OIL1"), 120.0; ("April", "OIL2"), 120.0; ("April", "OIL3"), 125.0;
    ("May", "VEG1"), 100.0; ("May", "VEG2"), 120.0; ("May", "OIL1"), 150.0; ("May", "OIL2"), 110.0; ("May", "OIL3"), 105.0;
    ("June", "VEG1"), 90.0; ("June", "VEG2"), 100.0; ("June", "OIL1"), 140.0; ("June", "OIL2"), 80.0; ("June", "OIL3"), 135.0;
]

let price = 150.00
let januaryPrices = prices.["January", All]

let oilAmount =
    [for x in januaryPrices do
        x.Key, Decision.createContinuous ("AmountOf" + x.Key) 0.0 infinity]
    |> Map.ofList

let y = (110.0*oilAmount.["VEG1"] + 120.0*oilAmount.["VEG2"] + 130.0*oilAmount.["OIL1"] + 110.0*oilAmount.["OIL2"] + 115.0*oilAmount.["OIL3"]) * (1.0 / 150.0)


let maxVeg = Constraint.create "MaximumVeg" (oilAmount.["VEG1"] + oilAmount.["VEG2"] <== 200.0)
let maxOil = Constraint.create "MaximumOil" (oilAmount.["OIL1"] + oilAmount.["OIL2"] + oilAmount.["OIL3"] <== 250.0)
let hardnessUpper = Constraint.create "HardnessUpper" (8.8*oilAmount.["VEG1"] + 6.1*oilAmount.["VEG2"] + 2.0*oilAmount.["OIL1"] + 4.2*oilAmount.["OIL2"] + 5.0*oilAmount.["OIL3"] - 6.0*y <== 0.0)
let hardnessLower = Constraint.create "HardnessLower" (8.8*oilAmount.["VEG1"] + 6.1*oilAmount.["VEG2"] + 2.0*oilAmount.["OIL1"] + 4.2*oilAmount.["OIL2"] + 5.0*oilAmount.["OIL3"] - 3.0*y >== 0.0)
let objective = Objective.create "MaximizeProfit" Maximize y
let weight = Constraint.create "WeightConstraint" (oilAmount.["VEG1"] + oilAmount.["VEG2"] + oilAmount.["OIL1"] + oilAmount.["OIL2"] + oilAmount.["OIL3"] - y == 0.0)

let model =
    // printfn (januaryPrices.ToString())
    Model.create objective
    |> Model.addConstraint maxVeg
    |> Model.addConstraint maxOil
    |> Model.addConstraint hardnessUpper
    |> Model.addConstraint hardnessLower
    |> Model.addConstraint weight

// Solve
let result = Solver.solve Settings.basic model

// Call the `solve` function in the Solve module to evaluate the model
//let result = Solver.solve settings model

[<EntryPoint>]
let main argv =
    printfn "-- Result --"
    // Match the result of the call to solve
    // If the model could not be solved it will return a `Suboptimal` case with a message as to why
    // If the model could be solved, it will print the value of the Objective Function and the
    // values for the Decision Variables
    match result with
    | Optimal solution ->
        printfn "Objective Value: %f" (Objective.evaluate solution objective)

        for (decision, value) in solution.DecisionResults |> Map.toSeq do
            let (DecisionName name) = decision.Name
            printfn "Decision: %s\tValue: %f" name value
    | _ -> printfn $"Unable to solve. Error: %A{result}"
    0 // return an integer exit code