﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using SaintCoinach.Ex.Relational.Serialization;
using SaintCoinach.Ex.Relational.ValueConverters;

using YamlDotNet.Serialization;
using Newtonsoft.Json.Linq;

namespace SaintCoinach.Ex.Relational.Definition {
    public class RelationDefinition {
        #region Fields

        private bool _IsCompiled;
        private ICollection<SheetDefinition> _SheetDefinitions = new List<SheetDefinition>();
        private Dictionary<string, SheetDefinition> _SheetMap;
        private Dictionary<string, SheetDefinition> _SheetDefinitionMap = new Dictionary<string, SheetDefinition>();

        #endregion

        #region Properties

        public ICollection<SheetDefinition> SheetDefinitions {
            get { return _SheetDefinitions; }
            internal set { _SheetDefinitions = value; }
        }

        public string Version { get; set; }

        #endregion

        #region Compile

        public void Compile() {
            _SheetMap = _SheetDefinitions.ToDictionary(_ => _.Name, _ => _);

            foreach (var sheet in SheetDefinitions)
                sheet.Compile();

            _IsCompiled = true;
        }

        #endregion

        #region Helpers

        public bool TryGetSheet(string name, out SheetDefinition def) {
            if (_IsCompiled)
                return _SheetMap.TryGetValue(name, out def);

            return _SheetDefinitionMap.TryGetValue(name, out def);
        }

        public SheetDefinition GetOrCreateSheet(string name) {
            if (!TryGetSheet(name, out var def)) {
                def = new SheetDefinition { Name = name };
                _SheetDefinitions.Add(def);
                _SheetDefinitionMap[name] = def;
            }
            return def;
        }

        #endregion

        #region Serialization

        public JObject ToJson() {
            return new JObject {
                ["version"] = Version,
                ["sheets"] = new JArray(_SheetDefinitions.OrderBy(s => s.Name).Select(s => s.ToJson()))
            };
        }

        public static RelationDefinition FromJson(JToken obj) {
            return new RelationDefinition() {
                Version = (string)obj["version"],
                SheetDefinitions = new List<SheetDefinition>(obj["sheets"].Select(j => SheetDefinition.FromJson(j)))
            };
        }

        public static RelationDefinition FromJson(string json) {
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
            return RelationDefinition.FromJson(obj);
        }

        public void Serialize(string filePath) {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                Serialize(writer);
        }

        public void Serialize(TextWriter writer) {
            // Sort definitions before serializing.
            _SheetDefinitions = _SheetDefinitions.OrderBy(s => s.Name).ToList();

            var serializer = new ExRelationSerializer();
            serializer.Serialize(writer, this);
        }

        public static RelationDefinition Deserialize(string filePath) {
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
                return Deserialize(reader);
        }

        public static RelationDefinition Deserialize(TextReader reader) {
            var deserializer = new Deserializer();

            deserializer.RegisterTagMapping("tag:yaml.org,2002:multiref_conv", typeof(MultiReferenceConverter));
            deserializer.RegisterTagMapping("tag:yaml.org,2002:ref_conv", typeof(GenericReferenceConverter));
            deserializer.RegisterTagMapping("tag:yaml.org,2002:color_conv", typeof(ColorConverter));
            deserializer.RegisterTagMapping("tag:yaml.org,2002:icon_conv", typeof(IconConverter));
            deserializer.RegisterTagMapping("tag:yaml.org,2002:link_conv", typeof(SheetLinkConverter));
            deserializer.RegisterTagMapping("tag:yaml.org,2002:tomestone_item_conv", typeof(TomestoneOrItemReferenceConverter));

            deserializer.RegisterTagMapping("tag:yaml.org,2002:group_def", typeof(GroupDataDefinition));
            deserializer.RegisterTagMapping("tag:yaml.org,2002:repeat_def", typeof(RepeatDataDefinition));
            deserializer.RegisterTagMapping("tag:yaml.org,2002:single_def", typeof(SingleDataDefinition));

            return deserializer.Deserialize<RelationDefinition>(reader);
        }

        #endregion
    }
}
