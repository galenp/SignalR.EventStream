SignalR.EventStream
-------------------
SignalR.EventStream was born out of a desire to monitor my websites
for activities that I was interested in. User signups, logins, errors,
anything really. SignalR from David Fowler and Damian Edwards has made
that nice and easy.


Page specific streams
---------------------
If you'd like to update a specific page (for any connected client) such as adding new comments or updating a view count. This utilizes the path information from the url to determine the group this connection belongs to.

    <script type="text/javascript">
        $(function () {
            var stream = new PageStream().connect();
            //this would be the same as calling
            // var stream = new EventStream(window.location.pathname).connect();

            stream.eventReceived = function (type, data) {
                var li = $("<li>");
                li.html(data.Message);
                li.appendTo("#messages");
            };
        });
    </script>
    <ul id="messages"></ul>

You can then send a message to anyone subscribed to this page via the route path.

    new EventStream().SendTo("/home/index", "status-update", new { Message = "some message" });


Group specific streams
----------------------
Similar to above however instead of using PageStream() you would use EventStream(groupName).

    <script type="text/javascript">
        $(function () {
            var stream = new EventStream("messages").connect();
            //this would be the same as calling
            // var stream = new EventStream(window.location.pathname).connect();

            stream.eventReceived = function (type, data) {
                var li = $("<li>");
                li.html(data.Message);
                li.appendTo("#messages");
            };
        });
    </script>
    <ul id="messages"></ul>


You can then send a message to anyone subscribed to this page via the route path.

    new EventStream().SendTo("messages", "status-update", new { Message = "some message" });


