using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class HttpAddresses
{
    public const string Lands = "http://54.75.253.236:11000/planets/jupiter/1";
    public const string Regions = "http://54.75.253.236:11000/planets/jupiter/regions";
    public const string Player1 = "http://54.75.253.236:11000/player/player1";
    public const string Player2 = "http://54.75.253.236:11000/player/player2";
    public const string Player3 = "http://54.75.253.236:11000/player/player3";

    public const string Move = "http://54.75.253.236:11000/lands/add-monster-to-land-by-cell-id/";

    public static void ErrorHandling(UnityWebRequest webRequest, string url)
    {
        string[] pages = url.Split('/');
        int page = pages.Length - 1;

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ProtocolError:
                throw new System.InvalidOperationException(pages[page] + ": HTTP Error: " + webRequest.error);
            default:
                throw new System.InvalidOperationException(pages[page] + ": Error:" + webRequest.error);
        }
    }
}
