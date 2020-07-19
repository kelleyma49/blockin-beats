#if ENABLE_UNITTESTS
using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ExcelDataReader;

namespace Logic.Tests
{
    public class StatsInfo
    {
        public int? Frame { get; set; }
        public int? FrameSquaresRemoved { get; set; }
        public int? NumPrevBonuses { get; set; }
        public float? DropPieceDownDropRate { get; set; }
        public int? TotalSquaresRemoved { get; set; }
        public int? TotalSquaresBonuses { get; set; }
        public int? TotalNumSingleColorBonuses { get; set; }
        public int? TotalNumEmptyColorBonuses { get; set; }
        public bool DebuggerBreak { get; set; }

        public void CheckStat(int? expected, int actual, string message)
        {
            if (expected != null)
            {
                var e = (int)expected;
                Assert.AreEqual(e, actual, $"{message}; expected {e}, actual {actual}");
            }
        }
    }

    public class StateInfo
    {
        public StateInfo(Playfield pf, DropPiece dp, float tl, StatsInfo si)
        {
            Playfield = pf;
            DropPiece = dp;
            Timeline = tl;
            Stats = si;
        }

        public Playfield Playfield { get; }
        public DropPiece DropPiece { get; }
        public float Timeline { get; }
        public StatsInfo Stats { get; }
    }

    class ExcelLoader : IEnumerable<StateInfo>
    {
        static ExcelLoader()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        public ExcelLoader(string name)
        {
            var deploymentFolder = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);
            var spreadsheetPath = Path.IsPathRooted(name) ? name : System.IO.Path.Combine(deploymentFolder, System.IO.Path.GetFileName(name));

            if (!System.IO.File.Exists(spreadsheetPath))
                throw new FileNotFoundException($"File not found: {spreadsheetPath}", spreadsheetPath);

            using (FileStream stream = File.Open(spreadsheetPath, FileMode.Open, FileAccess.Read))
            {
                using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream))
                {
                    //excelReader.Col = false;
                    _spreadSheet = excelReader.AsDataSet();
                }
            }
        }

        public StateInfo GenerateInitial()
        {
            return GeneratePlayfield(_spreadSheet.Tables["Initial"], false);
        }

        public IEnumerator<StateInfo> GetEnumerator()
        {
            for (int i = 0; i < _spreadSheet.Tables.Count; i++)
            {
                yield return GeneratePlayfield(_spreadSheet.Tables[i], true);
            }
        }

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            // Lets call the generic version here
            return this.GetEnumerator();
        }

        #endregion

        private static readonly string[] Keywords = new string[]
        {
            "timeline",
            "frame",
            "dropPieceDownDropRate",
            "totalSquaresRemoved",
            "totalNumEmptyColorBonuses",
            "frameSquaresRemoved",
            "debuggerBreak"
        };

        public StateInfo GeneratePlayfield(DataTable table, bool resetSubscriptions)
        {
            float? timeLine = null;
            var statsInfo = new StatsInfo();


            // count columns and rows:
            int columnCount = -1;
            int rowCount = 0;
            for (int ri = 0; ri < table.Rows.Count; ri++)
            {
                var row = table.Rows[ri];
                string str = row[0].ToString();

                // skip rows with empty first columns:
                if (String.IsNullOrWhiteSpace(str))
                    continue;

                // see if we've reached the end:
                if (str.StartsWith("timeline"))
                {
                    timeLine = Convert.ToSingle(row[1]);
                    continue;
                }
                else if (str.StartsWith("frame"))
                {
                    statsInfo.Frame = Convert.ToInt32(row[1]);
                    continue;
                }
                else if (str.StartsWith("dropPieceDownDropRate"))
                {
                    statsInfo.DropPieceDownDropRate = Convert.ToSingle(row[1]);
                    continue;
                }
                else if (str.StartsWith("totalSquaresRemoved"))
                {
                    statsInfo.TotalSquaresRemoved = Convert.ToInt32(row[1]);
                    continue;
                }
                else if (str.StartsWith("totalNumEmptyColorBonuses"))
                {
                    statsInfo.TotalNumEmptyColorBonuses = Convert.ToInt32(row[1]);
                    continue;
                }
                else if (str.StartsWith("totalNumSingleColorBonuses"))
                {
                    statsInfo.TotalNumSingleColorBonuses = Convert.ToInt32(row[1]);
                    continue;
                }
                else if (str.StartsWith("frameSquaresRemoved"))
                {
                    statsInfo.FrameSquaresRemoved = Convert.ToInt32(row[1]);
                    continue;
                }
                else if (str.StartsWith("debuggerBreak"))
                {
                    statsInfo.DebuggerBreak = true;
                    continue;
                }
                else if (str.StartsWith("key"))
                {
                    break; // we stop processing
                }
                else if (String.IsNullOrWhiteSpace(str))
                {
                    continue;
                }

                ++rowCount;

                // count columns - don't trust library as it 
                // sometimes reports more columns than expected:
                for (int c = 0; c < table.Columns.Count; c++)
                {
                    var col = row[c].ToString();
                    // skip rows with empty first columns:
                    if (String.IsNullOrWhiteSpace(col))
                    {
                        columnCount = Math.Max(c, columnCount);
                        break;
                    }
                }
            }

            // column count matches library's count:
            if (columnCount < 0)
                columnCount = table.Columns.Count;

            var pf = new Playfield(columnCount, rowCount);
            if (resetSubscriptions)
                pf.ResetSubscriptions();

            DropPiece dp = null;

            // fill playfield:
            int r = 0;
            int? columnDropPieceStart = null, rowDropPieceStart = null;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                if (r >= rowCount)
                    break;

                // skip rows with empty first columns:
                if (String.IsNullOrWhiteSpace(row[0].ToString()))
                    continue;

                if (Keywords.Contains(row[0]))
                {
                    r++;
                    continue;
                }
                int c = 0;
                for (int j = 0; j < columnCount; j++)
                {
                    var column = row[j];
                    var columnStr = column.ToString();
                    int dpCol = 0, dpRow = 0;

                    if (String.IsNullOrWhiteSpace(columnStr))
                        continue;

                    if (columnStr.StartsWith("DP_"))
                    {
                        if (columnDropPieceStart == null)
                            columnDropPieceStart = c;
                        if (rowDropPieceStart == null)
                            rowDropPieceStart = r;
                        dpCol = c - (int)columnDropPieceStart;
                        dpRow = r - (int)rowDropPieceStart;

                        if (dp == null)
                        {
                            dp = new DropPiece(new DropPieceSimple());
                            if (statsInfo.DropPieceDownDropRate != null)
                            {
                                dp.DownDropRate = statsInfo.DropPieceDownDropRate;
                            }
                        }
                        dp.Positions[dpCol] = new PlayfieldPoint(c, r);
                    }


                    var columnSplit = columnStr.Split('_');
                    switch (columnSplit[0])
                    {
                        case "D":
                            pf.Cells[c][r].State = Logic.Cell.States.Disabled;
                            break;

                        case "E":
                            pf.Cells[c][r].State = Logic.Cell.States.Empty;
                            break;

                        case "B":
                            pf.Cells[c][r].State = Logic.Cell.States.Black;
                            break;

                        case "W":
                            pf.Cells[c][r].State = Logic.Cell.States.White;
                            break;

                        case "BW":
                            pf.Cells[c][r].State = Logic.Cell.States.BlackAndWhite;
                            break;

                        // jeweled
                        case "JB":
                            pf.Cells[c][r].State = Logic.Cell.States.BlackJeweledBoth;
                            break;

                        case "JHB":
                            pf.Cells[c][r].State = Logic.Cell.States.BlackJeweledHorz;
                            break;

                        case "JVB":
                            pf.Cells[c][r].State = Logic.Cell.States.BlackJeweledVert;
                            break;

                        case "JW":
                            pf.Cells[c][r].State = Logic.Cell.States.WhiteJeweledBoth;
                            break;

                        case "JHW":
                            pf.Cells[c][r].State = Logic.Cell.States.WhiteJeweledHorz;
                            break;

                        case "JVW":
                            pf.Cells[c][r].State = Logic.Cell.States.WhiteJeweledVert;
                            break;

                        // drop piece
                        case "DP":
                            {
                                switch (columnSplit[1])
                                {
                                    case "W":
                                        dp.Cells[dpCol][dpRow] = Logic.Cell.States.White;
                                        break;

                                    case "B":
                                        dp.Cells[dpCol][dpRow] = Logic.Cell.States.Black;
                                        break;

                                    case "JW":
                                        dp.Cells[dpCol][dpRow] = Logic.Cell.States.WhiteJeweledBoth;
                                        break;

                                    case "JB":
                                        dp.Cells[dpCol][dpRow] = Logic.Cell.States.BlackJeweledBoth;
                                        break;
                                }
                            }
                            break;

                        default:
                            {
                                var sb = new System.Text.StringBuilder();
                                sb.AppendLine($"Unknown column type '{columnSplit[0]}' from '" +
                                    $"{columnStr}' at column {j }, row {i}");
                                sb.AppendLine($"in Worksheet '{table}'");
                                throw new InvalidDataException(sb.ToString());
                            }
                    }

                    c++;
                }
                r++;
            }

            // second pass - set remove states after cell states are set:
            r = 0;
            columnDropPieceStart = null; rowDropPieceStart = null;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                if (r >= rowCount)
                    break;

                // skip rows with empty first columns:
                if (String.IsNullOrWhiteSpace(row[0].ToString()))
                    continue;

                if (Keywords.Contains(row[0]))
                {
                    r++;
                    continue;
                }
                int c = 0;
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    var column = row[j];
                    var columnStr = column.ToString();

                    var columnSplit = columnStr.Split('_');
                    if (columnSplit[0] != "DP" && columnSplit.Length == 2)
                    {
                        switch (columnSplit[1])
                        {
                            case "NR":
                                pf.Cells[c][r].RemoveState = Logic.Cell.RemoveStates.NotRemoved;
                                break;
                            case "WR":
                                pf.Cells[c][r].RemoveState = Logic.Cell.RemoveStates.WillBeRemoved;
                                break;
                            case "RM":
                                pf.Cells[c][r].RemoveState = Logic.Cell.RemoveStates.Removing;
                                break;
                            case "JW":
                                pf.Cells[c][r].RemoveState = Logic.Cell.RemoveStates.JewelWillBeRemoved;
                                break;
                            case "JR":
                                pf.Cells[c][r].RemoveState = Logic.Cell.RemoveStates.JewelRemoving;
                                break;
                            default:
                                throw new InvalidDataException($"unknown remove state '{columnSplit[1]}'");
                        }
                    }
                    c++;
                }
                r++;
            }
            return new StateInfo(pf, dp, (float)timeLine, statsInfo);
        }

        public static void LoadAndRun(string file)
        {
            file = file.Replace("/", "" + Path.DirectorySeparatorChar).Replace("\\", "" + Path.DirectorySeparatorChar);
            var el = new ExcelLoader(file);
            var initial = el.GenerateInitial();

            var pf = initial.Playfield;
            var dp = initial.DropPiece;
            var prevTime = initial.Timeline;
            pf.Timeline.PositionAbs = initial.Timeline * pf.Timeline.NumColumns;

            foreach (var c in pf.GetEnumeratorCells())
            {
                c.Transferred += (sender, e) =>
                {
                    Assert.IsTrue(e.Cell.IsOccupied);
                    Assert.IsFalse((sender as Cell).IsOccupied);
                };
            }

            foreach (var v in el)
            {
                if (v.Stats.DebuggerBreak)
                    System.Diagnostics.Debugger.Break();

                // update playfield:
                pf.Timeline.IncrementPosition(v.Timeline - prevTime);

                string cellStateActual = pf.PrintCells("\t");
                string cellStateExpected = v.Playfield.PrintCells("\t");

                string msgCells = $"Cells aren't equal at time step {v.Timeline}, column ${pf.Timeline}.{Environment.NewLine}";

                msgCells += "Expected State:" + Environment.NewLine;
                msgCells += cellStateExpected + Environment.NewLine;
                msgCells += "Actual State:" + Environment.NewLine;
                msgCells += cellStateActual + Environment.NewLine;

                Assert.IsTrue(pf.GetEnumeratorCells().SequenceEqual(v.Playfield.GetEnumeratorCells()), msgCells);
                Assert.IsTrue(pf.GetEnumeratorSquares().SequenceEqual(v.Playfield.GetEnumeratorSquares()), $"Squares aren't equal at time step {v.Timeline}");

                string timeMsg = $"At time {v.Timeline}, ";

                // check drop piece:
                if (v.DropPiece != null && dp != null)
                {
                    dp.Update(TimeSpan.FromSeconds(v.Timeline - prevTime),
                        DropPiece.MoveDirection.None,
                        (c, r, i) => c >= 0 && c < pf.NumCellColumns, (md) => { },
                        (p) => p.Row >= pf.NumCellRows || pf.GetCell(p).IsOccupied,
                        (p, s) => pf.GetCell(p).State = s);


                    if (dp.CurrentState == DropPiece.State.Whole || dp.CurrentState == DropPiece.State.SplitLeft)
                    {
                        Assert.AreEqual(v.DropPiece.Cells[0][0], dp.Cells[0][0], $"{timeMsg} Cell doesn't match");
                        Assert.AreEqual(v.DropPiece.Cells[0][1], dp.Cells[0][1], $"{timeMsg} Cell doesn't match");
                    }
                    if (dp.CurrentState == DropPiece.State.Whole || dp.CurrentState == DropPiece.State.SplitRight)
                    {
                        Assert.AreEqual(v.DropPiece.Cells[1][0], dp.Cells[1][0], $"{timeMsg} Cell doesn't match");
                        Assert.AreEqual(v.DropPiece.Cells[1][1], dp.Cells[1][1], $"{timeMsg} Cell doesn't match");
                    }

                    Assert.AreEqual(2, dp.Positions.Length);
                    if (dp.CurrentState == DropPiece.State.Whole || dp.CurrentState == DropPiece.State.SplitLeft)
                        Assert.AreEqual(v.DropPiece.Positions[0], dp.Positions[0], $"{timeMsg} Positions don't match");
                    if (dp.CurrentState == DropPiece.State.Whole || dp.CurrentState == DropPiece.State.SplitRight)
                        Assert.AreEqual(v.DropPiece.Positions[1], dp.Positions[1], $"{timeMsg} Positions don't match");
                }

                // check stats info:
                StatsInfo si = v.Stats;
                si.CheckStat(si.Frame, pf.Stats.Frame, $"{timeMsg} Frame doesn't match");
                si.CheckStat(si.FrameSquaresRemoved, pf.Stats.FrameSquaresRemoved, $"{timeMsg} FrameSquaresRemoved doesn't match");
                si.CheckStat(si.NumPrevBonuses, pf.Stats.NumPrevBonuses, $"{timeMsg} NumPrevBonuses doesn't match");
                si.CheckStat(si.TotalSquaresRemoved, pf.Stats.TotalSquaresRemoved, $"{timeMsg} TotalSquaresRemoved doesn't match");
                si.CheckStat(si.TotalSquaresBonuses, pf.Stats.TotalSquaresBonuses, $"{timeMsg} TotalSquaresBonuses doesn't match");
                si.CheckStat(si.TotalNumSingleColorBonuses, pf.Stats.TotalNumSingleColorBonuses, $"{timeMsg} TotalNumSingleColorBonuses doesn't match");
                si.CheckStat(si.TotalNumEmptyColorBonuses, pf.Stats.TotalNumEmptyColorBonuses, $"{timeMsg} TotalNumEmptyColorBonuses doesn't match");

                prevTime += v.Timeline - prevTime;
            }
        }

        private readonly DataSet _spreadSheet;
    }
}
#endif // ENABLE_UNITTESTS
