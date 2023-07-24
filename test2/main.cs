using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using System.Windows.Forms;
using System.Drawing;
using Grasshopper.Kernel.Attributes;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace test2
{
    public class main : GH_Component
                 
    {
        ///作業ディレクトリの指定
        public string path = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Grasshopper\Libraries\config.json";
        public int length;
        public string setting_path = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Grasshopper\Libraries\setting.json";
        //public string FilePath = @"C:\Program Files (x86)\KKE\RESP-D\ModelCalcCtrlApp.exe";//実行ファイルの場所
        //public string FilePathFolder = @"C:\Program Files (x86)\KKE\RESP-D";//RESPフォルダ場所の指定
        public string RespFile;//RESP-Dファイルの場所
        public string ThreadNum; //スレッド数
        // Lengthプロパティの宣言
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public main()
          : base("Run RESP-D script", "Run RESP-D script",
              "Make RESP-D script with JSON style and run it.",
              "ParaParaRESP", "3.Analysis")
        {
    }
        public override void CreateAttributes()
        {
            m_attributes = new Attributes_Custom(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Plan", "Plan", "Input design plan of damper design", GH_ParamAccess.list);
            pManager.AddGenericParameter("OutputDirectory", "OutputDirectory", "Input the directory of RESP-D analysisfile", GH_ParamAccess.item);
            pManager.AddGenericParameter("dzFileName", "dzFileName", "Input original RESP-D file name", GH_ParamAccess.item);
            pManager.AddGenericParameter("ThreadNumber", "ThreadNumber", "Input thread number of time history analysis", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {        }

        //Dscriptの定義
        public class Script
        {
            public string OutputDirectory { get; set; }
            public List<dynamic> Plans { get; set; }
           // public List<ParaPlan> ParaPlan { get; set; }
        }
        public class Member
        {
            public string MemberType { get; set; }
            public string MemberSubType { get; set; }
            public List<string> Floor { get; set; }
            public List<string> Frame { get; set; }
            public List<string> Axis { get; set; }
            public List<ActionQueries> ActionQueries { get; set; }
        }
        public class ActionQueries
        {
            public string QueryType { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string ValueType { get; set; }
        }
        public class Plan
        {
            public string Code { get; set; }
            public string Description { get; set; }
            public List<Member> Members { get; set; }
            public object ParaQuery { get; set; }
        }

        ///パラスタプランを定義するオブジェクト
        //public class ParaPlan
        //{
        //    public string Code { get; set; }
        //    public string Discription { get; set; }
        //    public object ParaQuery { get; set; }
        //}
        public class ParaQuery
        {
            public string StrategyType { get; set; }
            public List<MemberR> Members { get; set; }
            public object Limitation { get; set; }
            public List<Range> Range { get; set; }

        }
        public class MemberR
        {
            public string MemberType { get; set; }
            // public string MemberSubType { get; set; }
            public List<string> Floor { get; set; }
            public List<string> Frame { get; set; }
            public List<string> Axis { get; set; }
        }
        public class Limitation
        {
            public string DamperCategory { get; set; }
            public string Mark { get; set; }
            public string Arrangement { get; set; }
            public string Direction { get; set; }
        }
        public class Range
        {
            public string RangeType { get; set; }
            public double Lower { get; set; }
            public double Upper { get; set; }
            public double Interval { get; set; }
        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        public class CaseNum
        {
            public int Num { get; set; }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            ///Resp-D member jsonオブジェクトの作成
            List<dynamic> plans = new List<dynamic>();
            DA.GetDataList("Plan", plans);
            length = plans.Count;
            DA.GetData("dzFileName", ref RespFile);

            //Script情報の作成
            string outputdirectry = "";
            DA.GetData("OutputDirectory", ref outputdirectry);

            //解析時のスレッド数の取得
            DA.GetData("ThreadNumber", ref ThreadNum);

            var script = new Script
            {
                OutputDirectory = outputdirectry,
                Plans = new List<dynamic>(),
            //    ParaPlan = new List<ParaPlan>()
            };

            //受け取った全てのplans情報を展開
            if (plans != null)
            {
                for (int i = 0; i < length; i++)
                {
                    var temp = plans[i];
                    // JSON形式に変換
                    var tem = JsonConvert.SerializeObject(temp, Formatting.Indented);
                    // JSON形式データの要素を抽出
                    JObject jObject = JObject.Parse(tem);

                    if ((string)jObject["Value"]["Code"] != "Para" ) // パラスタプラン以外の場合の読み込みフロー
                    {
                        Plan plan1 = new Plan
                        {
                            Code = (string)jObject["Value"]["Code"],
                            Description = (string)jObject["Value"]["Discription"],
                            Members = new List<Member>()
                        };

                        foreach (var member in jObject["Value"]["Members"])
                        {
                            Member member1 = new Member
                            {
                                MemberType = (string)member["MemberType"],
                                MemberSubType = (string)member["MemberSubType"],
                                Floor = ((JArray)member["Floor"]).ToObject<List<string>>(),
                                Frame = ((JArray)member["Frame"]).ToObject<List<string>>(),
                                Axis = ((JArray)member["Axis"]).ToObject<List<string>>(),
                                ActionQueries = new List<ActionQueries>()
                            };
                            ActionQueries query = new ActionQueries
                            {
                                QueryType = (string)member["ActionQueries"][0]["QueryType"],
                                Name = (string)member["ActionQueries"][0]["Name"],
                                Value = (string)member["ActionQueries"][0]["Value"],
                                ValueType = (string)member["ActionQueries"][0]["ValueType"]
                            };
                            // 各データをクラスに代入する
                            member1.ActionQueries.Add(query);
                            plan1.Members.Add(member1);
                        };
                        script.Plans.Add(plan1);
                    }
                    else //パラスタプランの場合の読み込みフロー
                    {
                        Plan para_plan1 = new Plan
                        {
                            Code = (string)jObject["Value"]["Code"] + i.ToString(),
                            Description = (string)jObject["Value"]["Discription"],
                            ParaQuery = new ParaQuery
                            {
                            StrategyType = (string)jObject["Value"]["ParaQuery"]["StrategyType"],
                                Members = new List<MemberR>() {
                            new MemberR
                            {
                            MemberType = (string)jObject["Value"]["ParaQuery"]["Members"][0]["MemberType"],
                            //MemberSubType = (string)jObject["Value"]["ParaQuery"]["Members"][0]["MemberSubType"],
                            Floor = jObject["Value"]["ParaQuery"]["Members"][0]["Floor"].ToObject<List<string>>(),
                            Frame = jObject["Value"]["ParaQuery"]["Members"][0]["Frame"].ToObject<List<string>>(),
                            Axis = jObject["Value"]["ParaQuery"]["Members"][0]["Axis"].ToObject<List<string>>(),
                            }}
                            ,
                                Limitation = new Limitation
                            {
                                DamperCategory = (string)jObject["Value"]["ParaQuery"]["Limitation"]["DamperCategory"],
                                Mark = (string)jObject["Value"]["ParaQuery"]["Limitation"]["Mark"],
                                Arrangement = (string)jObject["Value"]["ParaQuery"]["Limitation"]["Arrangement"],
                                Direction = (string)jObject["Value"]["ParaQuery"]["Limitation"]["Direction"],
                            },
                                Range = new List<Range>() {
                            new Range
                            {
                            RangeType = (string)jObject["Value"]["ParaQuery"]["Range"][0]["RangeType"],
                            Lower = double.Parse((string)jObject["Value"]["ParaQuery"]["Range"][0]["Lower"]),
                            Upper = double.Parse((string)jObject["Value"]["ParaQuery"]["Range"][0]["Upper"]),
                            Interval = double.Parse((string)jObject["Value"]["ParaQuery"]["Range"][0]["Interval"]),
                            }
                            },
                    }
                        };
                        script.Plans.Add(para_plan1);
                    }
                };
            };

            // JSONに変換して出力する
            var json = JsonConvert.SerializeObject(script, Formatting.Indented);
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(json);
            }

        //GenerateがTrueならResp-Dscriptをバッチ実行
        }

        //Runボタンの作成とResp-Dscriptの実行
        public class Attributes_Custom : GH_ComponentAttributes
        {
            public Attributes_Custom(GH_Component owner) : base(owner) {
            }

            protected override void Layout()
            {
                base.Layout();

                Rectangle rec0 = GH_Convert.ToRectangle(Bounds);
                rec0.Height += 66;

                Rectangle rec1 = rec0;
                rec1.Y = rec1.Bottom - 66;
                rec1.Height = 22;
                rec1.Inflate(-2, -2);

                Rectangle rec2 = rec0;
                rec2.Y = rec0.Bottom - 44;
                rec2.Height = 22;
                rec2.Inflate(-2, -2);

                Rectangle rec3 = rec0;
                rec3.Y = rec0.Bottom - 22;
                rec3.Height = 22;
                rec3.Inflate(-2, -2);

                Bounds = rec0;
                ButtonBounds2 = rec3;
                ButtonBounds = rec2;
                TextBounds = rec1;
            }
            private Rectangle ButtonBounds { get; set; }
            private Rectangle ButtonBounds2 { get; set; }
            private Rectangle TextBounds { get; set; } // テキストの境界線を作成

            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                var temp = Owner as main;
                base.Render(canvas, graphics, channel);
                if (channel == GH_CanvasChannel.Objects)
                {
                    GH_Capsule button2 = GH_Capsule.CreateTextCapsule(ButtonBounds2, ButtonBounds2, GH_Palette.Black, "Run", 2, 0);
                    button2.Render(graphics, Selected, Owner.Locked, false);
                    button2.Dispose();
                    GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "CreateModel", 2, 0);
                    button.Render(graphics, Selected, Owner.Locked, false);
                    button.Dispose();
                    // テキストの作成
                    string _text = "Parametric Case:" + temp.length;//解析ケース数の出力
                    GH_Capsule text = GH_Capsule.CreateTextCapsule(TextBounds, TextBounds, GH_Palette.Transparent, _text, 2, 0);
                    text.Render(graphics, Selected, Owner.Locked, false);
                    text.Dispose();
                }
            }

            public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                var temp = Owner as main;
                //setting.jsonから初期設定を取得
                string jsonContent = File.ReadAllText(temp.setting_path);
                // JSONをパースしてオブジェクトに変換
                JObject jsonObject = JObject.Parse(jsonContent);
                // RESPPathの値を取得
                string respPath = (string)jsonObject["RESPPath"][0];

                if (e.Button == MouseButtons.Left)
                {
                    RectangleF rec2 = ButtonBounds2;
                    if (rec2.Contains(e.CanvasLocation))
                    {

                        // 実行するバッチコードの情報を設定します
                        string arguments = " --run -E -D -I " + temp.RespFile + " --script "+temp.path+" --bunshin -L "+temp.ThreadNum + " -T "+temp.ThreadNum;

                        MessageBox.Show("Run Analysis of Resp-D file.", "", MessageBoxButtons.OK);

                        // バッチファイルの実行開始
                        ProcessStartInfo processInfo = new ProcessStartInfo(respPath, arguments);
                        processInfo.RedirectStandardOutput = true;
                        processInfo.UseShellExecute = false;

                        Process process = Process.Start(processInfo);

                        return GH_ObjectResponse.Handled;
                    }
                    else
                    {
                        RectangleF rec = ButtonBounds;
                        if (rec.Contains(e.CanvasLocation))
                        {
                            // 実行するバッチコードの情報を設定します
                            string arguments = "-I " + temp.RespFile + " --script "+ temp.path;

                            MessageBox.Show("Create Analysis model of Resp-D file.", "", MessageBoxButtons.OK);

                            // バッチファイルの実行開始
                            ProcessStartInfo processInfo = new ProcessStartInfo(respPath, arguments);
                            processInfo.RedirectStandardOutput = true;
                            processInfo.UseShellExecute = false;

                            Process process = Process.Start(processInfo);

                            return GH_ObjectResponse.Handled;
                        }
                    }
                }

                return base.RespondToMouseDown(sender, e);
            }
            //public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
            //{
            //    var temp = Owner as main;
            //    if (e.Button == MouseButtons.Left)
            //    {
            //        RectangleF rec2 = ButtonBounds2;
            //        if (rec2.Contains(e.CanvasLocation))
            //        {
            //            // 実行するバッチコードの情報を設定します
            //            string arguments = "-D "+ temp.RespFile + " --script config.json";

            //            MessageBox.Show("Run Analysis of Resp-D file.", "", MessageBoxButtons.OK);

            //            //バッチファイルの実行開始
            //            ProcessStartInfo processInfo = new ProcessStartInfo(temp.FilePath, arguments);
            //            processInfo.RedirectStandardOutput = true;
            //            processInfo.UseShellExecute = false;

            //            Process process = Process.Start(processInfo);

            //            return GH_ObjectResponse.Handled;
            //        }
            //    }
            //    return base.RespondToMouseDown(sender, e);
            //    if (e.Button == MouseButtons.Left)
            //    {
            //        RectangleF rec = ButtonBounds;
            //        if (rec.Contains(e.CanvasLocation))
            //        {
            //            // 実行するバッチコードの情報を設定します
            //            string arguments = "-I " + temp.RespFile + " --script config.json";

            //            MessageBox.Show("Create Analysis of Resp-D file.", "", MessageBoxButtons.OK);

            //            //バッチファイルの実行開始
            //            ProcessStartInfo processInfo = new ProcessStartInfo(temp.FilePath, arguments);
            //            processInfo.RedirectStandardOutput = true;
            //            processInfo.UseShellExecute = false;

            //            Process process = Process.Start(processInfo);

            //            return GH_ObjectResponse.Handled;
            //        }
            //    }
            //    return base.RespondToMouseDown(sender, e);
            //}
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return test2.Properties.Resources.run;
            }
        }


        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d3390b02-de22-4c49-ad90-cdd503f8ce15"); }
        }
    }
}
