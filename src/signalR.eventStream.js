function EventStream() {
    this.initialized = false;
    this.connect = function () {
        if (!this.initialized) {
            var stream = $.connection.eventStream;
            var parent = this;
            stream.receiveEvent = function (event) {
                var result = $.parseJSON(event);
                var template = $("#eventStream-template-" + result.Type);
                if (template && template.tmpl) {
                    parent.eventReceived(result, template.tmpl(result.Type == "event" ? result : result.Event));
                } else {
                    parent.eventReceived(result);
                }
            };

            $.connection.hub.start({}, function () {
                stream.authorize()
                    .fail(function (e) {
                        $.connection.hub.stop();
                        parent.connectionFailed(e);
                    })
                    .done(function (success) {
                        if (success === false) {
                            $.connection.hub.stop();
                            parent.connectionFailed("unauthorized");
                        }
                    });
            });

            this.initialized = true;
        }

        return this;
    };

    this.eventReceived = function (data, template) {
        $("body").append(template);
    };

    this.connectionFailed = function (e) {
        alert('unable to connect to event stream.\r\nError:' + e);
    };
};