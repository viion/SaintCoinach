﻿using System.Linq;

using SaintCoinach.Ex.Relational.Definition;
using SaintCoinach.IO;

namespace SaintCoinach.Ex.Relational {
    public class RelationalExCollection : ExCollection {
        #region Fields

        private RelationDefinition _Definition = new RelationDefinition();

        #endregion

        #region Properties

        public RelationDefinition Definition { get { return _Definition; } set { _Definition = value; } }

        #endregion

        #region Constructors

        #region Constructor

        public RelationalExCollection(PackCollection packCollection) : base(packCollection) { }

        #endregion

        #endregion

        #region Factory

        protected override Header CreateHeader(string name, File file) {
            return new RelationalHeader(this, name, file);
        }

        protected override ISheet CreateSheet(Header header) {
            var relHeader = (RelationalHeader)header;
            if (relHeader.Variant == 2)
                return CreateSheet<Variant2.RelationalDataRow>(relHeader);
            return CreateSheet<Variant1.RelationalDataRow>(relHeader);
        }

        private ISheet CreateSheet<T>(RelationalHeader header) where T : IRelationalDataRow {
            if (header.AvailableLanguagesCount >= 1)
                return new RelationalMultiSheet<RelationalMultiRow, T>(this, header);
            return new RelationalDataSheet<T>(this, header, header.AvailableLanguages.First());
        }

        #endregion

        #region Get

        public new IRelationalSheet<T> GetSheet<T>(int id) where T : IRelationalRow {
            return (IRelationalSheet<T>)GetSheet(id);
        }

        public new IRelationalSheet GetSheet(int id) {
            return (IRelationalSheet)base.GetSheet(id);
        }

        public new IRelationalSheet<T> GetSheet<T>(string name) where T : IRelationalRow {
            return (IRelationalSheet<T>)GetSheet(name);
        }

        public new IRelationalSheet GetSheet(string name) {
            return (IRelationalSheet)base.GetSheet(name);
        }

        #endregion


        #region Find

        public IRelationalRow FindReference(int key) {
            // Optimization for unlikely references.
            if (key <= 0)
                return null;

            foreach (var sheetDef in Definition.SheetDefinitions.Where(d => d.IsGenericReferenceTarget)) {
                var sheet = GetSheet(sheetDef.Name);
                if (!sheet.Header.DataFileRanges.Any(r => r.Contains(key)))
                    continue;

                if (!sheet.ContainsRow(key))
                    continue;

                return sheet[key];
            }
            return null;
        }

        #endregion
    }
}
