// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
"use strict";

const generatePlayerCardV3 = (player) => `
<div class="mx-2 mt-2 mb-3 d-flex flex-row" id="${player.playerID}">
    <div class="player-icon" id="status-${player.playerID}" data-avatar="${player.avatarID}"></div>
    <div class="ml-3 player-name">${player.player}</div>
</div>`;

const labels = ['0', '½', '1', '2', '3', '5', '8', '13', '21', '40', '100', '?'];
const PHASE = Object.freeze({ Created: 0, Voting: 1, Results: 2, Finished: 3 });

/* Extracts a parameter from the url. */
const readParameter = (param) => {
    const result = window.location.search
        .substring(1)
        .split('&')
        .map(v => v.split('='))
        .find(e => e[0] == param);
    return result != undefined ? result[1] : undefined;
}

/* creates a chart from the data and context. */
const createChart = (ctx, data) => {
    return new Chart(ctx, {
        type: 'doughnut',
        data: {
            datasets: [{
                data: data,
                backgroundColor: [
                    "#FFFF00", "#FFAE42", "#FFA500", "#FF4500",
                    "#FF0000", "#922B3E", "#EE82EE", "#324AB2",
                    "#0000FF", "#0D98BA", "#008000", "#9ACD32",
                ]
            }],
            labels: labels,
        },
        options: {
            legend: {
                display: false,
            },
            responsive: true,
            animation: {
                animateScale: true,
                animateRotate: true
            },
            plugins: {
                labels: [
                    {
                        render: function (args) {
                            return `${args.label} SP`;
                        },
                        fontSize: 16,
                        fontStyle: 'bold',
                        fontColor: '#FFF',
                    }
                ]
            }
        }
    });
}

/* maps a set of PVI structures to their vote count. */
const mapVotesToCountAndSetDisplay = (arrayOfVoteInfo) => {
    var votesNum = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    arrayOfVoteInfo.forEach(vi => {
        if (vi.vote !== null) {
            votesNum[vi.vote] += 1;
            $(`#status-${vi.playerID}`).attr("data-vote", labels[vi.vote]);
        }
    });
    return votesNum;
}

/* Reusable error log function. */
const logCaughtErr = (err) => console.error(err.toString());

$(document).ready(() => {
    var myChart = undefined;
    var currentPhase = PHASE.Created;
    var token = readParameter('t');

    /*
     * Authenticate.
     */

    /* get a hub connection. */
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/app/pokergamehub", { accessTokenFactory: () => token })
        .withAutomaticReconnect()
        .build();

    /*
     * Configure the connection responses.
     */

    connection.start().then(() => {
        connection.onclose(e => {
            $('#disconnectedModalLong').modal({
                backdrop: 'static',
                keyboard: false
            });
        });
    }).catch(logCaughtErr);

    /* received when a join occurs (even our own.) */
    connection.on("PlayerJoined", playerConnectedResponse => {
        if ($('#playerlist').has(`#${playerConnectedResponse.playerID}`).length == 0) {
            var playerCard = generatePlayerCardV3(playerConnectedResponse);
            $("#playerlist").append(playerCard);
        } else {
            $(`#status-${playerConnectedResponse.playerID}`).removeClass('disconnected');
        }
    });

    /* Received when a user disconnects. */
    connection.on("PlayerDisconnected", playerDisconnectedResponse => {
        $(`#status-${playerDisconnectedResponse.playerID}`).addClass("disconnected");
    });

    /* Received when a user has been disconnected for too long. */
    connection.on("PlayerPurged", playerPurgedResponse => {
        $(`#${playerPurgedResponse.playerID}`).remove()
    })

    /* Received in response to joining a room. */
    connection.on("ReceiveGameStateFromJoinRoom", playerConnectedRequiresGameStateResponse => {
        playerConnectedRequiresGameStateResponse.players.forEach(pd => {
            if (!$('#playerlist').has(`#${pd.playerID}`).length) {
                var playerCard = generatePlayerCardV3(pd);
                $("#playerlist").append(playerCard);
            }

            var id = `#status-${pd.playerID}`;
            var e = $(id);
            if (pd.isModerator && !e.hasClass('moderator')) {
                e.addClass('moderator');
            }

            if (pd.isObserver && !e.hasClass('observer')) {
                e.addClass('observer');
            }

            if (pd.vote != null) {
                switch (playerConnectedRequiresGameStateResponse.phase) {
                    case PHASE.Results:
                        e.attr('data-vote', labels[pd.vote]);
                        e.addClass('display-vote');
                    case PHASE.Voting:
                    case PHASE.Created:
                        e.addClass('voted');
                        break;
                    default:
                        break;
                }
            }
        });

        /* We also need to prepare the chart, if we're switching to results mode. */
        if (playerConnectedRequiresGameStateResponse.phase === PHASE.Results && currentPhase != PHASE.Results) {
            var ctx = document.getElementById('resultsChart').getContext('2d');
            var votesNum = mapVotesToCountAndSetDisplay(playerConnectedRequiresGameStateResponse.players);
            myChart = createChart(ctx, votesNum);

            // populate the stats
            $('#votes-card').html(playerConnectedRequiresGameStateResponse.players.filter( v => v.vote != null ).length);
            $('#min-card').html(labels[playerConnectedRequiresGameStateResponse.low]);
            $('#max-card').html(labels[playerConnectedRequiresGameStateResponse.high]);
            $('#mode-card').html(labels[playerConnectedRequiresGameStateResponse.mode]);

            $('#results-tab').click();
        }

        //
        currentPhase = playerConnectedRequiresGameStateResponse.phase;

        document.getElementById("leaveRoomButton").disabled = false;
        $('#loginModal').modal('hide');
        $('#loader-complete').remove();
    });

    /* Received when a vote is cast. */
    connection.on("PlayerVoted", sendVoteResponse => {
        var id = `#status-${sendVoteResponse.playerID}`;
        $(id).addClass('voted');
    });

    /* Received when a moderator instructs a reveal. */
    connection.on("StartDiscussionPhase", sendResultResponse => {
        currentPhase = PHASE.Results;

        var votesNum = mapVotesToCountAndSetDisplay(sendResultResponse.votes);
        var ctx = document.getElementById('resultsChart').getContext('2d');
        if (myChart === undefined) {
            myChart = createChart(ctx, votesNum);
        } else {
            myChart.data.datasets[0].data = votesNum;
            myChart.update();
        }

        $('.player-icon[data-vote]').addClass('display-vote');
        $('#votes-card').html(sendResultResponse.votes.length);
        $('#min-card').html(labels[sendResultResponse.low]);
        $('#max-card').html(labels[sendResultResponse.high]);
        $('#mode-card').html(labels[sendResultResponse.mode]);

        /* switch to results tab. */
        $('#results-tab').click();
    });

    /* Received when a moderator instructs a reset. */
    connection.on("StartVotingPhase", beginVotingResponse => {
        currentPhase = PHASE.Voting;
        $('.vote').removeClass('active');
        $('.vote').removeClass('focus');
        $('.player-icon').removeClass('voted');
        $('.player-icon').removeClass('display-vote');
        $('.player-icon').attr("data-vote", null);
        $('#vote-tab').click();
    });

    /* Received when a player toggles the observer status. */
    connection.on("PlayerChanged", sendPlayerChangedResponse => {
        var id = `#status-${sendPlayerChangedResponse.playerID}`;

        sendPlayerChangedResponse.isObserver ? $(id).addClass('observer') : $(id).removeClass('observer');
        sendPlayerChangedResponse.isModerator ? $(id).addClass('moderator') : $(id).removeClass('moderator');
    });

    /*
     * Configure SPA actions.
     */

    /* Attempts to issue a vote. */
    $('.vote').click(function (event) {
        $('.vote').removeClass('active');
        $(this).addClass('active');

        connection.invoke("SendVote", {
            Vote: $(this).attr('data-value')
        }).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });

    /* Issues a request for all participants to be shown the votes. */
    $('#revealVote').click(function (event) {
        connection.invoke("SendResult", { }).catch(function (err) {
            return console.error(err.toString());
        });
    });

    /* Issues a request for all participants to reset their vote. */
    $('#resetVote').click(function (event) {
        connection.invoke("BeginVoting", { }).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });

    /* Inform the server that this player is now a/not a moderator. */
    $('#playerIsModerator').change(function () {
        this.checked ? $("#playareamain").addClass("moderator") : $("#playareamain").removeClass("moderator");
        connection.invoke("SendPlayerChange", {
            Observer: null,
            Moderator: this.checked
        }).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });

    /* Inform the server that this player is now an/not an observer. */
    $('#playerIsObserver').change(function () {
        connection.invoke("SendPlayerChange", {
            Observer: this.checked,
            Moderator: null
        }).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });

    /* Return to home page. */
    $('#relogButton').click(function () {
        var url = `${window.location.protocol}//${window.location.host}/`;
        window.location = url;
    });
});

$(function () {
    $('[data-toggle="tooltip"]').tooltip({
        delay: {
            "show": 100,
            "hide": 100
        }
    });
});
