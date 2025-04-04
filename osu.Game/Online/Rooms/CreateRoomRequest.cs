﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    public class CreateRoomRequest : APIRequest<APICreatedRoom>
    {
        public readonly Room Room;

        public CreateRoomRequest(Room room)
        {
            Room = room;

            // Also copy back to the source model, since it is likely to have been stored elsewhere.
            Success += r => Room.CopyFrom(r);
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.ContentType = "application/json";
            req.Method = HttpMethod.Post;
            req.AddRaw(JsonConvert.SerializeObject(Room));

            return req;
        }

        protected override string Target => "rooms";
    }
}
