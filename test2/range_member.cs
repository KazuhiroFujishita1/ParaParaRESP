using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace test2
{
    public class range_member : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the range_member class.
        /// </summary>
        public range_member()
          : base("SelectMemberRange", "SelectMemberRange",
              "Input Member range information",
              "ParaParaRESP", "0.Setting")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            int idx;
            pManager.AddTextParameter("MemberType", "MemberType", "Input member infomation.", GH_ParamAccess.item);
            idx = pManager.AddTextParameter("MemberSubType", "MemberSubType", "Input member infomation.", GH_ParamAccess.item);
            pManager[idx].Optional = true;
            idx = pManager.AddGenericParameter("LowerFloor", "LowerFloor", "Input Floor range information", GH_ParamAccess.item);
            pManager[idx].Optional = true;
            idx = pManager.AddGenericParameter("UpperFloor", "UpperFloor", "Input Floor range information", GH_ParamAccess.item);
            pManager[idx].Optional = true;
            idx = pManager.AddGenericParameter("StartFrame", "StartFrame", "Input Frame range information", GH_ParamAccess.item);
            pManager[idx].Optional = true;
            idx = pManager.AddGenericParameter("EndFrame", "EndFrame", "Input Frame range information", GH_ParamAccess.item);
            pManager[idx].Optional = true;
            idx = pManager.AddGenericParameter("StartAxis", "StartAxis", "Input Axis range information", GH_ParamAccess.item);
            pManager[idx].Optional = true;
            idx = pManager.AddGenericParameter("EndAxis", "EndAxis", "Input Axis range information", GH_ParamAccess.item);
            pManager[idx].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Target", "Target", "Connect to the placeincrement component", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // grasshopperから情報を読み込み
            //var floor = new List<string>();
            //var frame = new List<string>();
            //var axis = new List<string>();
            string lowerfloor="";
            string upperfloor ="";
            string startframe = "";
            string endframe = "";
            string startaxis = "";
            string endaxis = "";
            string membertype = "";
            string membersubtype = "";
            string querytype = "";
            string name = "";
            string value = "";
            string valuetype = "";

            DA.GetData("LowerFloor", ref lowerfloor);
            DA.GetData("UpperFloor", ref upperfloor);
            DA.GetData("StartFrame", ref startframe);
            DA.GetData("EndFrame", ref endframe);
            DA.GetData("StartAxis", ref startaxis);
            DA.GetData("EndAxis", ref endaxis);

            DA.GetData("MemberType", ref membertype);
            if (Params.Input[1] != null)
            {
                DA.GetData("MemberSubType", ref membersubtype);
            }
            else
            {
                membersubtype = "";
            }

            //位置のリストを作成
            //if (lowerfloor <= upperfloor)
            // {
            //     floor.Add(lowerfloor.ToString()+"F");
            //     floor.Add(upperfloor.ToString() + "F");
            // }
            // else
            //{
            //    floor.Add(upperfloor.ToString() + "F");
            //    floor.Add(lowerfloor.ToString() + "F");
            //}
            //if (startframe <= endframe)
            //{
            //    frame.Add("X"+startframe.ToString()) ;
            //    frame.Add("X" + endframe.ToString());
            //}
            //else
            //{
            //    frame.Add("X" + endframe.ToString());
            //    frame.Add("X" + startframe.ToString());
            //}
            //if (startaxis <= endaxis)
            //{
            //    axis.Add("Y" + startaxis.ToString());
            //    axis.Add("Y" + endaxis.ToString());
            //}
            //else
            //{
            //    axis.Add("Y" + endaxis.ToString());
            //    axis.Add("Y" + startaxis.ToString());
            //}
            List<string> floor = new List<string>() { lowerfloor, upperfloor };
            List<string> frame = new List<string>() { startframe, endframe };
            List<string> axis = new List<string>() { startaxis, endaxis };


            //全てのメンバー情報を構造体に挿入
            List<object> member = new List<object>();
            var members = new Member
            {
                MemberType = membertype,
                MemberSubType = membersubtype,
                Floor = floor,
                Frame = frame,
                Axis = axis,
                ActionQueries = new List<ActionQuery> {
                    new ActionQuery {
                        QueryType = querytype,
                        Name = name,
                        Value = value,
                        ValueType = valuetype
                    }
                }
            };

            // grasshopperへ出力
            DA.SetData(0, members);
        }

        // JSONの型定義
        public class Member
        {
            public string MemberType { get; set; }
            public string MemberSubType { get; set; }
            public List<string> Floor { get; set; }
            public List<string> Frame { get; set; }
            public List<string> Axis { get; set; }
            public List<ActionQuery> ActionQueries { get; set; }
        }
        public class ActionQuery
        {
            public string QueryType { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string ValueType { get; set; }
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
                return test2.Properties.Resources.place;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("602226c2-dba6-4e51-a7c6-1bd76043da33"); }
        }
    }
}