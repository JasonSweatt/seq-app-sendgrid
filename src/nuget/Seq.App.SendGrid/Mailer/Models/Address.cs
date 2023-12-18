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

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return Name == null ? EmailAddress : $"{Name} <{EmailAddress}>";
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
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