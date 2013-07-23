using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.interfaces;
using umbraco.uicontrols;
using Umbraco.Core.IO;

namespace Refactored.UmbracoEmailExtensions.DataTypes.MessageBoard
{
    public class DataTypeMessageBoard : umbraco.cms.businesslogic.datatype.BaseDataType, IDataType
    {
        private IDataEditor _editor;
        private umbraco.cms.businesslogic.datatype.DefaultData _baseData;
        private IDataPrevalue _prevalueeditor;

        public override IDataEditor DataEditor
        {
            get { return _editor ?? (_editor = new MessageBoard()); }
        }

        public override IData Data
        {
            get { return _baseData ?? (_baseData = new umbraco.cms.businesslogic.datatype.DefaultData(this)); }
        }

        public override Guid Id { get { return new Guid("D1387545-7437-4036-9D25-B6C5F63C0045"); } }

        public override string DataTypeName { get { return "Email Extensions Message Board"; } }

        public override IDataPrevalue PrevalueEditor
        {
            get { return _prevalueeditor ?? (_prevalueeditor = new umbraco.editorControls.DefaultPrevalueEditor(this, false)); }
        }
    }
}
