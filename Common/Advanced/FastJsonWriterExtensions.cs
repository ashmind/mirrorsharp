using System;
using System.Collections.Immutable;
using System.Text;
using MirrorSharp.Internal;

namespace MirrorSharp.Advanced {
    /// <summary>
    /// Provides common helper methods for <see cref="IFastJsonWriter"/>.
    /// </summary>
    public static class FastJsonWriterExtensions {
        /// <summary>Writes a new JSON property with a string value (e.g. <c>"name": "value"</c>).</summary>
        /// <param name="writer">Writer to write the property to.</param>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write; can be null.</param> 
        public static void WriteProperty(this IFastJsonWriter writer, string name, string? value) {
            Argument.NotNull(nameof(writer), writer);
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }

        /// <summary>Writes a new JSON property with a string value (e.g. <c>"name": "value"</c>).</summary>
        /// <param name="writer">Writer to write the property to.</param>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write; can be null.</param> 
        public static void WriteProperty(this IFastJsonWriter writer, string name, StringBuilder? value) {
            Argument.NotNull(nameof(writer), writer);
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }        
        
        /// <summary>Writes a new JSON property with a string value (e.g. <c>"name": "value"</c>).</summary>
        /// <param name="writer">Writer to write the property to.</param>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write.</param> 
        public static void WriteProperty(this IFastJsonWriter writer, string name, ArraySegment<char> value) {
            Argument.NotNull(nameof(writer), writer);
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }
        
        /// <summary>Writes a new JSON property with a string value (e.g. <c>"name": "value"</c>).</summary>
        /// <param name="writer">Writer to write the property to.</param>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write; can be null.</param> 
        public static void WriteProperty(this IFastJsonWriter writer, string name, ImmutableArray<char> value) {
            Argument.NotNull(nameof(writer), writer);
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }

        /// <summary>Writes a new JSON property with a single-character string value (e.g. <c>"name": "c"</c>).</summary>
        /// <param name="writer">Writer to write the property to.</param>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write.</param>
        public static void WriteProperty(this IFastJsonWriter writer, string name, char value) {
            Argument.NotNull(nameof(writer), writer);
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }

        /// <summary>Writes a new JSON property with an integer value (e.g. <c>"name": 1</c>).</summary>
        /// <param name="writer">Writer to write the property to.</param>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write.</param>
        public static void WriteProperty(this IFastJsonWriter writer, string name, int value) {
            Argument.NotNull(nameof(writer), writer);
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }

        /// <summary>Writes a new JSON property with a boolean value (e.g. <c>"name": true</c>).</summary>
        /// <param name="writer">Writer to write the property to.</param>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write.</param>
        public static void WriteProperty(this IFastJsonWriter writer, string name, bool value) {
            Argument.NotNull(nameof(writer), writer);
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }

        /// <summary>Writes a new JSON property and opens its object value (e.g. <c>"name": {</c>).</summary>
        /// <param name="writer">Writer to write the property to.</param>
        /// <param name="name">Name of the property to write.</param>
        public static void WritePropertyStartObject(this IFastJsonWriter writer, string name) {
            Argument.NotNull(nameof(writer), writer);
            writer.WritePropertyName(name);
            writer.WriteStartObject();
        }

        /// <summary>Writes a new JSON property and opens its array value (e.g. <c>"name": [</c>).</summary>
        /// <param name="writer">Writer to write the property to.</param>
        /// <param name="name">Name of the property to write.</param>
        public static void WritePropertyStartArray(this IFastJsonWriter writer, string name) {
            Argument.NotNull(nameof(writer), writer);
            writer.WritePropertyName(name);
            writer.WriteStartArray();
        }
    }
}
