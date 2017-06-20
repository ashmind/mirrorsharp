using System;
using System.IO;
using JetBrains.Annotations;

namespace MirrorSharp.Advanced {
    /// <summary>
    /// JSON writer used to communicate with MirrorSharp clients.
    /// </summary>
    /// <remarks>
    /// At the moment the output is not actively validated --
    /// writer can produce invalid JSON if not used carefully.
    /// </remarks>
    [PublicAPI]
    public interface IFastJsonWriter : IDisposable {
        /// <summary>Opens a new JSON object (<c>{</c>).</summary>
        void WriteStartObject();

        /// <summary>Closes current JSON object (<c>}</c>).</summary>
        void WriteEndObject();

        /// <summary>Opens an new JSON array (<c>[</c>).</summary>
        void WriteStartArray();

        /// <summary>Closes current JSON array (<c>]</c>).</summary>
        void WriteEndArray();

        /// <summary>Writes a new JSON property with a string value (e.g. <c>"name": "value"</c>).</summary>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write; can be null.</param> 
        void WriteProperty([NotNull] string name, [CanBeNull] string value);

        /// <summary>Writes a new JSON property with a single-character string value (e.g. <c>"name": "c"</c>).</summary>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write.</param>
        void WriteProperty([NotNull] string name, char value);

        /// <summary>Writes a new JSON property with an integer value (e.g. <c>"name": 1</c>).</summary>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write.</param>
        void WriteProperty([NotNull] string name, int value);

        /// <summary>Writes a new JSON property with a boolean value (e.g. <c>"name": true</c>).</summary>
        /// <param name="name">Name of the property to write.</param>
        /// <param name="value">Value of the property to write.</param>
        void WriteProperty([NotNull] string name, bool value);

        /// <summary>Writes a new JSON property and opens its object value (e.g. <c>"name": {</c>).</summary>
        /// <param name="name">Name of the property to write.</param>
        void WritePropertyStartObject([NotNull] string name);

        /// <summary>Writes a new JSON property and opens its array value (e.g. <c>"name": [</c>).</summary>
        /// <param name="name">Name of the property to write.</param>
        void WritePropertyStartArray([NotNull] string name);

        /// <summary>Writes a new JSON property name (e.g. <c>"name":</c>).</summary>
        /// <param name="name">Name of the property to write.</param>
        void WritePropertyName([NotNull] string name);

        /// <summary>Writes <see cref="String" /> value as a JSON string.</summary>
        /// <param name="value">Value to write; can be null.</param>
        void WriteValue([CanBeNull] string value);

        /// <summary>Writes <see cref="Char" /> value as a JSON string.</summary>
        /// <param name="value">Value to write.</param>
        void WriteValue(char value);

        /// <summary>Writes <see cref="Int32" /> value as a JSON number.</summary>
        /// <param name="value">Value to write.</param>
        void WriteValue(int value);

        /// <summary>Writes <see cref="Boolean" /> value as a JSON boolean.</summary>
        /// <param name="value">Value to write.</param>
        void WriteValue(bool value);

        /// <summary>Writes a start <c>"</c> for a JSON string, and returns a <see cref="TextWriter" /> for writing its content.</summary>
        /// <returns>A writer for writing into the JSON string.</returns>
        /// <remarks>The returned writer should be disposed for the string to be closed properly.</remarks>
        TextWriter OpenString();
    }
}