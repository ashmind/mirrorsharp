using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeStaticMemberQualifier

/// <summary>
/// Provides methods for verification of argument preconditions.
/// </summary>
internal static class Argument {
// ReSharper restore CheckNamespace
    /// <summary>
    /// Verifies that a given argument value is not <c>null</c> and returns the value provided.
    /// </summary>
    /// <typeparam name="T">Type of the <paramref name="name" />.</typeparam>
    /// <param name="name">Argument name.</param>
    /// <param name="value">Argument value.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    /// <returns><paramref name="value"/> if it is not <c>null</c>.</returns>
    public static T NotNull<T>(string name, T value)
        where T : class
    {
        if (value == null)
            throw new ArgumentNullException(name);
        return value;
    }

    /// <summary>
    /// Verifies that a given argument value is not <c>null</c> and returns the value provided.
    /// </summary>
    /// <typeparam name="T">Type of the <paramref name="name" />.</typeparam>
    /// <param name="name">Argument name.</param>
    /// <param name="value">Argument value.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    /// <returns><paramref name="value"/> if it is not <c>null</c>.</returns>
    public static T NotNull<T>(string name, T? value)
        where T : struct
    {
        if (value == null)
            throw new ArgumentNullException(name);
        return value.Value;
    }

    /// <summary>
    /// Verifies that a given argument value is not <c>null</c> or empty and returns the value provided.
    /// </summary>
    /// <param name="name">Argument name.</param>
    /// <param name="value">Argument value.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is empty.</exception>
    /// <returns><paramref name="value"/> if it is not <c>null</c> or empty.</returns>
    public static string NotNullOrEmpty(string name, string value) {
        Argument.NotNull(name, value);
        if (value.Length == 0)
            throw NewArgumentEmptyException(name);

        return value;
    }

    /// <summary>
    /// Verifies that a given argument value is not <c>null</c> or empty and returns the value provided.
    /// </summary>
    /// <param name="name">Argument name.</param>
    /// <param name="value">Argument value.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is empty.</exception>
    /// <returns><paramref name="value"/> if it is not <c>null</c> or empty.</returns>
    public static T[] NotNullOrEmpty<T>(string name, T[] value) {
        Argument.NotNull(name, value);
        if (value.Length == 0)
            throw NewArgumentEmptyException(name);

        return value;
    }

    /// <summary>
    /// Verifies that a given argument value is not <c>null</c> or empty and returns the value provided.
    /// </summary>
    /// <param name="name">Argument name.</param>
    /// <param name="value">Argument value.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is empty.</exception>
    /// <returns><paramref name="value"/> if it is not <c>null</c> or empty.</returns>
    public static TCollection NotNullOrEmpty<TCollection>(string name, TCollection value)
        where TCollection : class, IEnumerable
    {
        Argument.NotNull(name, value);
        var enumerator = value.GetEnumerator();
        try {
            if (!enumerator.MoveNext())
                throw NewArgumentEmptyException(name);
        }
        finally {
            (enumerator as IDisposable)?.Dispose();
        }

        return value;
    }

    private const string PotentialDoubleEnumeration = "Using NotNullOrEmpty with plain IEnumerable may cause double enumeration. Please use a collection instead.";

    /// <summary>
    /// (DO NOT USE) Ensures that NotNullOrEmpty can not be used with plain <see cref="IEnumerable"/>,
    /// as this may cause double enumeration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete(PotentialDoubleEnumeration, true)]
    // ReSharper disable UnusedParameter.Global
    public static void NotNullOrEmpty(string name, IEnumerable value) {
    // ReSharper restore UnusedParameter.Global
        throw new Exception(PotentialDoubleEnumeration);
    }

    /// <summary>
    /// (DO NOT USE) Ensures that NotNullOrEmpty can not be used with plain <see cref="IEnumerable{T}" />,
    /// as this may cause double enumeration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete(PotentialDoubleEnumeration, true)]
    // ReSharper disable UnusedParameter.Global
    public static void NotNullOrEmpty<T>(string name, IEnumerable<T> value) {
    // ReSharper restore UnusedParameter.Global
        throw new Exception(PotentialDoubleEnumeration);
    }

    private static Exception NewArgumentEmptyException(string name) {
        return new ArgumentException("Value can not be empty.", name);
    }

    /// <summary>
    /// Casts a given argument into a given type if possible.
    /// </summary>
    /// <typeparam name="T">Type to cast <paramref name="value"/> into.</typeparam>
    /// <param name="name">Argument name.</param>
    /// <param name="value">Argument value.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> can not be cast into type <typeparamref name="T"/>.</exception>
    /// <returns><paramref name="value"/> cast into <typeparamref name="T"/>.</returns>
    public static T Cast<T>(string name, object value) {
        if (!(value is T))
            throw new ArgumentException(string.Format("The value \"{0}\" isn't of type \"{1}\".", value, typeof(T)), name);

        return (T)value;
    }

    /// <summary>
    /// Verfies that a given argument is not null and casts it into a given type if possible.
    /// </summary>
    /// <typeparam name="T">Type to cast <paramref name="value"/> into.</typeparam>
    /// <param name="name">Argument name.</param>
    /// <param name="value">Argument value.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> can not be cast into type <typeparamref name="T"/>.</exception>
    /// <returns><paramref name="value"/> cast into <typeparamref name="T"/>.</returns>
    public static T NotNullAndCast<T>(string name, object value) {
        Argument.NotNull(name, value);
        return Argument.Cast<T>(name, value);
    }

    /// <summary>
    /// Verifies that a given argument value is greater than or equal to zero and returns the value provided.
    /// </summary>
    /// <param name="name">Argument name.</param>
    /// <param name="value">Argument value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is less than zero.</exception>
    /// <returns><paramref name="value"/> if it is greater than or equal to zero.</returns>
    public static int PositiveOrZero(string name, int value) {
        if (value < 0) {
            // ReSharper disable once HeapView.BoxingAllocation
            throw new ArgumentOutOfRangeException(name, value, "Value must be positive or zero.");
        }

        return value;
    }

    /// <summary>
    /// Verifies that a given argument value is greater than zero and returns the value provided.
    /// </summary>
    /// <param name="name">Argument name.</param>
    /// <param name="value">Argument value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is less than or equal to zero.</exception>
    /// <returns><paramref name="value"/> if it is greater than zero.</returns>
    public static int PositiveNonZero(string name, int value) {
        if (value <= 0) {
            // ReSharper disable once HeapView.BoxingAllocation
            throw new ArgumentOutOfRangeException(name, value, "Value must be positive and not zero.");
        }

        return value;
    }
}
