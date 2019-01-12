module Fable.Import.MatchSorter


open Fable.Core
open Fable.Core.JsInterop


[<RequireQualifiedAccess>]
type Ranking =
  | CaseSensitiveEqual
  | Equal
  | StartsWith
  | WordStartsWith
  | StringCase
  | StringCaseAcronym
  | Contains
  | Acronym
  | Matches
  | NoMatch


type KeySpec<'a> =
  { Selector: 'a -> string
    MinRanking: Ranking option
    MaxRanking: Ranking option
    Threshold: Ranking option }


module KeySpec =

  let create selector =
    { Selector = selector
      MinRanking = None
      MaxRanking = None
      Threshold = None }

  let withMinRanking ranking keySpec =
    { keySpec with MinRanking = Some ranking }

  let withMaxRanking ranking keySpec =
    { keySpec with MaxRanking = Some ranking }

  let withThreshold ranking keySpec =
    { keySpec with Threshold = Some ranking }


type MSOpts<'a> =
  { Keys: KeySpec<'a> list
    Threshold: Ranking option
    KeepDiacritics: bool option }


module MSOpts =

  let empty<'a> : MSOpts<'a> =
    { Keys = []
      Threshold = None
      KeepDiacritics = None }

  let addKeySpec keySpec opts =
    { opts with Keys = opts.Keys @ [keySpec] }

  let addKey selector opts =
    addKeySpec (KeySpec.create selector) opts

  let withThreshold ranking (opts: MSOpts<_>) =
    { opts with Threshold = Some ranking }

  let keepDiacritics opts =
    { opts with KeepDiacritics = Some true }



module Native =

  module Rankings =

    type IExports =
      abstract CASE_SENSITIVE_EQUAL: float
      abstract EQUAL: float
      abstract STARTS_WITH: float
      abstract WORD_STARTS_WITH: float
      abstract STRING_CASE: float
      abstract STRING_CASE_ACRONYM: float
      abstract CONTAINS: float
      abstract ACRONYM: float
      abstract MATCHES: float
      abstract NO_MATCH: float


  let [<Import("rankings", "match-sorter")>] rankings: Rankings.IExports = jsNative


  type MinMaxRanking =
    abstract key: U2<string, ('T -> string)> with get, set
    abstract minRanking: float with get, set
    abstract maxRanking: float with get, set
    abstract threshold: float with get, set


  type Options<'T> =
    abstract keys: ResizeArray<U3<string, ('T -> string), MinMaxRanking>> with get, set
    abstract threshold: float with get, set
    abstract keepDiacritics: bool with get, set


  [<Import("default", "match-sorter")>]
  let matchSorter (items: ResizeArray<'T>) (value: string) (options: Options<'T>) : ResizeArray<'T> = jsNative


module Internal =

  open Native

  let convertRanking = function
    | Ranking.CaseSensitiveEqual -> rankings.CASE_SENSITIVE_EQUAL
    | Ranking.Equal -> rankings.EQUAL
    | Ranking.StartsWith -> rankings.STARTS_WITH
    | Ranking.WordStartsWith -> rankings.WORD_STARTS_WITH
    | Ranking.StringCase -> rankings.STRING_CASE
    | Ranking.StringCaseAcronym -> rankings.STRING_CASE_ACRONYM
    | Ranking.Contains -> rankings.CONTAINS
    | Ranking.Acronym -> rankings.ACRONYM
    | Ranking.Matches -> rankings.MATCHES
    | Ranking.NoMatch -> rankings.NO_MATCH


  let convertKeySpec (s: KeySpec<_>) : MinMaxRanking =
    jsOptions<MinMaxRanking>(fun o ->
      o.key <- !^s.Selector
      match s.MinRanking with Some x -> o.minRanking <- convertRanking x | _ -> ()
      match s.MaxRanking with Some x -> o.maxRanking <- convertRanking x | _ -> ()
      match s.Threshold with Some x -> o.threshold <- convertRanking x | _ -> ()
    )


  let convertMsOpts (mso: MSOpts<_>) : Options<'a> =
    jsOptions<Options<'a>>(fun o ->
      o.keys <- mso.Keys |> Seq.map (convertKeySpec >> (!^)) |> ResizeArray
      match mso.Threshold with Some x -> o.threshold <- convertRanking x | _ -> ()
      match mso.KeepDiacritics with Some x -> o.keepDiacritics <- x | _ -> ()
    )



/// Run match-sorter without options.
let matchSort query (items: seq<_>) =
  Native.matchSorter (ResizeArray items) query createEmpty
  |> Seq.toList

/// Run match-sorter with the specified options. The options are converted to the
/// internal match-sort options once they are partially applied.
let matchSortWith (options: MSOpts<_>) =
  let opts = Internal.convertMsOpts options
  fun value (items: seq<_>) ->
    Native.matchSorter (ResizeArray items) value opts
    |> Seq.toList
