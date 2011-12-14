function PageStream() {
    return new EventStream(window.location.pathname);
}
function EventStream(authorizeFor) {
    this.authorizeFor = authorizeFor;
    this.initialized = false;
    this.connect = function () {
        if (!this.initialized) {
            var stream = $.connection.eventStream;
            var parent = this;
            stream.receiveEvent = function (event) {
                var result = $.parseJSON(event);
                parent.eventReceived(result.Type, result.Event);
            };

            $.connection.hub.start({}, function () {
                stream.authorize(parent.authorizeFor)
                    .fail(function (e) {
                        $.connection.hub.stop();
                        parent.connectionFailed(e);
                    })
                    .done(function (success) {
                        if (success !== true) {
                            $.connection.hub.stop();
                            parent.unauthorized();
                        }
                    });
            });

            this.initialized = true;
        }

        return this;
    };

    this.eventReceived = function (type, data) {
    };

    this.connectionFailed = function (e) {
        alert('unable to connect to event stream.\r\nError:' + e);
    };

    this.unauthorized = function (e) {

    };
};