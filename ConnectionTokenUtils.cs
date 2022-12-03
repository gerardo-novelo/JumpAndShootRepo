using System;

// <summary>
// Fusion Connection Token Utility methods
// Se supone que hay un codigo en fusion que facilita el manejo de connection Tokens pero no lo encontre asi que hice un replica en el proyecto
// </summary

public static class ConnectionTokenUtils
{
    // Create new random Token
    public static byte[] NewToken() => Guid.NewGuid().ToByteArray();

    //Convert a Token into a Hash format
    public static int HashToken(byte[] token) => new Guid(token).GetHashCode();

    //Convert a Token into a String
    public static string TokenToString(byte[] token) => new Guid(token).ToString();
}