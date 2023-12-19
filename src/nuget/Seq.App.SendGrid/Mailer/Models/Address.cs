using System.Runtime.CompilerServices;

namespace Seq.App.SendGrid.Mailer.Models;

/// <summary>
/// Class Address.
/// </summary>
public class Address
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Address"/> class.
    /// </summary>
    public Address()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Address"/> class.
    /// </summary>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="name">The name.</param>
    public Address(string? emailAddress, string? name = null)
    {
        EmailAddress = emailAddress;
        Name = name;
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    /// <value>The email address.</value>
    public string? EmailAddress { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name == null ? EmailAddress : $"{Name} <{EmailAddress}>";
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        return RuntimeHelpers.GetHashCode(this);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj == null || !GetType().Equals(obj.GetType()))
        {
            return false;
        }

        var otherAddress = (Address)obj;
        return EmailAddress == otherAddress.EmailAddress && Name == otherAddress.Name;
    }
}