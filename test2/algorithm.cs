using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Grasshopper.Kernel.Types;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Base;
using Grasshopper.GUI;
using System.Windows.Forms;
using Grasshopper.Kernel.Attributes;
using System.Drawing;

namespace test2
{
    public class algorithm : GH_Component
    {
        public string x1;
        public string x2;
        public string y1;
        public string y2;
        public string z1;
        public string z2;
        

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public algorithm()
          : base("DamperDesignAlgorithm", "DamperDesignAlgorithm",
              "Increment candidate arrangement plan from base plans",
              "ParaParaRESP", "1.Plan")
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
            int idx;
            ///pManager.AddGenericParameter("ParametricTarget", "ParametricTarget", "Input Target parameter of parametric study.", GH_ParamAccess.list);
            idx = pManager.AddGenericParameter("Target", "Target", "Input target of damper design", GH_ParamAccess.item);
            idx = pManager.AddGenericParameter("Strategy", "Strategy", "Input the strategy of damper design.", GH_ParamAccess.item);
            idx = pManager.AddGenericParameter("Direction", "Direction", "Set the limitation.", GH_ParamAccess.item);
            pManager[idx].Optional = false;
            //idx = pManager.AddGenericParameter("RangeType", "RangeType", "Control the number of generate plan.", GH_ParamAccess.item);
            idx = pManager.AddNumberParameter("UpperLimit", "UpperLimit", "Control the number of generate plan.", GH_ParamAccess.item);
            idx = pManager.AddNumberParameter("LowerLimit", "LowerLimit", "Control the number of generate plan.", GH_ParamAccess.item);
            idx = pManager.AddNumberParameter("Interval", "Interval", "Control the number of generate plan.", GH_ParamAccess.item);
            //idx = pManager.AddGenericParameter("DamperCategory", "DamperCategory", "Set the limitation.", GH_ParamAccess.item);
            //idx = pManager.AddGenericParameter("Mark", "Mark", "Set the limitation.", GH_ParamAccess.item);
            //idx = pManager.AddGenericParameter("Arrangement", "Arrangement", "Set the limitation.", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Name", "Name", "Input the method of operation of damper design", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Value", "Value", "Input the method of operation of damper design", GH_ParamAccess.item);
            //pManager.AddGenericParameter("ValueType", "ValueType", "Input the method of operation of damper design", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Plan", "Plan", "Output generated design plans.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        /// 
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
        ///パラスタプランを定義するオブジェクト
        public class ParaPlan
        {
            public string Code { get; set; }
            public string Discription { get; set; }
            public object ParaQuery { get; set; }
        }
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

        /// </summary>
        /// <param name="DA"></param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///Resp-D member jsonオブジェクトの作成
            string dampercategory = "";
            string mark = "";
            string arrangement = "";
            string rangetype = "";
            double lower = 0;
            double upper = 0;
            double interval = 0;
            string direction = "";
            object member = new Member();


            if (Params.Input[1] != null)
            {
                DA.GetData("Target", ref member);
            }
            else
            {
                Member specificMember = (Member)member;
                specificMember.MemberType = "";
                specificMember.MemberSubType = "";
                specificMember.Floor = new List<string>() { "", "" };
                specificMember.Frame = new List<string>() { "", "" };
                specificMember.Axis = new List<string>() { "", "" };
                specificMember.ActionQueries = new List<ActionQueries>();
                member = specificMember;
            }

            var temp1 = JsonConvert.SerializeObject(member, Formatting.Indented);

            string method = "";
            DA.GetData("Strategy", ref method);

            //DA.GetData("DamperCategory", ref dampercategory);
            //DA.GetData("Mark", ref mark);
            //DA.GetData("Arrangement", ref arrangement);
            //DA.GetData("RangeType", ref rangetype);
            //OilDamperCapacityをスタディする場合
            if (method == "VelocityDependentDamperCapacity")
            {
                method = "DamperCapacity";
                dampercategory = "VelocityDependent";
                arrangement = "Brace";
                rangetype = "Capacity";
                mark = "*";
            }
            else if (method == "DamperRatio")//Damperratioをスタディする場合
            {
                dampercategory = "";
                arrangement = "Brace";
                rangetype = "Ratio";
                mark = "*|*";
            }
            else if (method == "HysteresisDamperCapacity")            //HysteresisDamperCapacityをスタディする場合
            {
                method = "DamperCapacity";
                dampercategory = "Hysteresis";
                arrangement = "Brace";
                rangetype = "Capacity";
                mark = "*";
            }


            DA.GetData("LowerLimit", ref lower);
            DA.GetData("UpperLimit", ref upper);
            DA.GetData("Interval", ref interval);
            DA.GetData("Direction", ref direction);

            // JSON形式データの要素を抽出
            JObject jObject = JObject.Parse(temp1);
            ///  DA.GetDataList("ParametricTarget", parametric_tar);

            //UIへの範囲指定出力
            //if (Params.Input[0] != null)
            //{
            x1 = (string)jObject["Value"]["Floor"][0];
            x2 = (string)jObject["Value"]["Floor"][1];
            y1 = (string)jObject["Value"]["Frame"][0];
            y2 = (string)jObject["Value"]["Frame"][1];
            z1 = (string)jObject["Value"]["Axis"][0];
            z2 = (string)jObject["Value"]["Axis"][1];
            //}

            //パラスタの実行定義を生成
            var para_plan = new ParaPlan
            {
                Code = "Para",
                Discription = "",
                ParaQuery = new ParaQuery
                {
                    StrategyType = method,
                    Members = new List<MemberR> {
                        new MemberR
                        {
                            MemberType = (string)jObject["Value"]["MemberType"],
                            Floor = jObject["Value"]["Floor"].ToObject<List<string>>(),
                            Frame = jObject["Value"]["Frame"].ToObject<List<string>>(),
                            Axis = jObject["Value"]["Axis"].ToObject<List<string>>(),
                        }
                    },
                    Limitation = new Limitation
                    {
                        DamperCategory = dampercategory,
                        Mark = mark,
                        Arrangement = arrangement,
                        Direction = direction
                    },
                    Range = new List<Range>
                {
                    new Range
                    {
                        RangeType = rangetype,
                        Lower = lower,
                        Upper = upper,
                        Interval = interval
                    }
                }
                },
            };

            // grasshopperへ出力
            DA.SetData("Plan", para_plan);

        }

        public class Attributes_Custom : GH_ComponentAttributes
        {
            public Attributes_Custom(GH_Component owner) : base(owner)
            {
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
                TextBounds = rec1;
                TextBounds2 = rec2;
                TextBounds3 = rec3;
            }
            private Rectangle TextBounds { get; set; } // テキストの境界線を作成
            private Rectangle TextBounds2 { get; set; } // テキストの境界線を作成
            private Rectangle TextBounds3 { get; set; } // テキストの境界線を作成

            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                var aaa = Owner as algorithm;

                base.Render(canvas, graphics, channel);
                if (channel == GH_CanvasChannel.Objects)
                {
                    GH_Capsule textbox = GH_Capsule.CreateTextCapsule(TextBounds, TextBounds, GH_Palette.White, aaa.x1+"    Floor    "+ aaa.x2, 2, 0);
                    textbox.Render(graphics, Selected, Owner.Locked, false);
                    textbox.Dispose();
                    GH_Capsule textbox2 = GH_Capsule.CreateTextCapsule(TextBounds2, TextBounds2, GH_Palette.White, aaa.y1+ "    Frame    "+ aaa.y2, 2, 0);
                    textbox2.Render(graphics, Selected, Owner.Locked, false);
                    textbox2.Dispose();
                    GH_Capsule textbox3 = GH_Capsule.CreateTextCapsule(TextBounds3, TextBounds3, GH_Palette.White, aaa.z1 + "    Axis    "+ aaa.z2, 2, 0);
                    textbox3.Render(graphics, Selected, Owner.Locked, false);
                    textbox3.Dispose();
                    // テキストの作成
                }
            }

            public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    RectangleF rec = TextBounds;
                    if (rec.Contains(e.CanvasLocation))
                    {
                    }
                }
                return base.RespondToMouseDown(sender, e);
            }

        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return test2.Properties.Resources.parametric;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d5b85b71-092f-40b4-af79-7f2875dfb4f5"); }
        }
    }
}