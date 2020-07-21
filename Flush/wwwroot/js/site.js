// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
"use strict";

const generatePlayerCardV2 = (user) => `
<div class="mb-2 mx-1">
    <div class="input-group" id="card-${user.playerId}">
        <div class="input-group-prepend mod" id="${ user.playerId}">
            <span class="input-group-text bg-light mod"><i class="fas fa-fw" id="rank-${user.playerId}"></i></span >
        </div>
        <input type="text" class="form-control bg-light text-dark" id="text-${user.playerId}" disabled value="${user.name}"/>
        <div class="input-group-append">
            <span class="input-group-text nameplate" id="status-${user.playerId}"><i class="fas vote-fw voteplate text-dark" id="vote-${user.playerId}"></i></span >
        </div>
    </div>
</div>`;

const labels = ['0', '&frac12;', '1', '2', '3', '5', '8', '13', '21', '40', '100', '?'];
const PHASE = Object.freeze({ Created: 0, Voting: 1, Results: 2, Finished: 3 });

/* Extracts a parameter from the url */
const readParameter = (param) => {
    const result = window.location.search
        .substring(1)
        .split('&')
        .map(v => v.split('='))
        .find(e => e[0] == param);
    return result != undefined ? result[1] : undefined;
}

/* creates a chart from the data and context */
const createChart = (ctx, data) => {
    return new Chart(ctx, {
        type: 'pie',
        data: {
            datasets: [{
                data: data,
                backgroundColor: [
                    "#C047EB", "#311872", "#6E1926", "#C31E56",
                    "#C4FA20", "#A6B0AF", "#6BD532", "#B8AA69",
                    "#AC6950", "#14CAFD", "#A61B38", "#F4A483",
                ]
            }],
            labels: labels,
        },
        options: {
            legend: {
                display: false,
            },
            responsive: true
        }
    });
}

/* maps a set of PVI structures to their vote count */
const mapVotesToCountAndSetDisplay = (arrayOfPlayerVoteInfo) => {
    var votesNum = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    arrayOfPlayerVoteInfo.forEach(pvi => {
        if (pvi.vote > -1) {
            votesNum[pvi.vote] += 1;
            $(`#vote-${pvi.playerId}`).html(labels[pvi.vote]);
        }
    });
    return votesNum;
}

/* Reusable error log function */
const logCaughtErr = (err) => console.error(err.toString());

$(document).ready(() => {
    var myChart = undefined;
    var playerIsObserver = false;
    var currentPhase = PHASE.Created;
    var token = readParameter('t');
    var room = readParameter('r');

    /*
     * Authenticate
     */

    /* get a hub connection */
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/app/pokergamehub", { accessTokenFactory: () => token })
        .withAutomaticReconnect()
        .build();

    /*
     * Configure the connection responses
     */

    connection.start().then(() => {
        connection.onclose(e => {
            $('#relogModal').modal({
                backdrop: 'static',
                keyboard: false
            });
        });
    }).catch(logCaughtErr);

    /* received when a join occurs (even our own) */
    connection.on("PlayerJoined", playerInfo => {
        if ($('#playerlist').has(`#${playerInfo.playerId}`).length == 0) {
            var playerCard = generatePlayerCardV2(playerInfo);
            $("#playerlist").append(playerCard);
        }
    });

    /* Received in response to joining a room */
    connection.on("ReceiveGameStateFromJoinRoom", gameStateInfo => {
        currentPhase = gameStateInfo.phase;
        gameStateInfo.players.forEach(psi => {
            if (!$('#playerlist').has(`#${$.escapeSelector(psi.playerId)}`).length) {
                var playerCard = generatePlayerCardV2(psi);
                $("#playerlist").append(playerCard);
                if (gameStateInfo.phase === PHASE.Voting || gameStateInfo.phase === PHASE.Created) {
                    var id = `#status-${psi.playerId}`;
                    /* A vote value below zero is an informational state */
                    if (psi.vote != null) {
                        $(id).addClass('bg-success');
                        $(id).addClass('border-success');
                    }
                }
                else if (gameStateInfo.phase === PHASE.Results) {
                    /* Insert the players vote. */
                    if (psi.vote != null) {
                        var id = `#vote-${psi.playerId}`;
                        $(id).html(labels[psi.vote]);
                    }
                }
            }
        });

        /* We also need to prepare the chart, if we're switching to results mode */
        if (gameStateInfo.phase === PHASE.Results) {
            var ctx = document.getElementById('resultsChart').getContext('2d');
            var votesNum = mapVotesToCountAndSetDisplay(gameStateInfo.players);
            myChart = createChart(ctx, votesNum);
            $('#results-tab').click();
        }

        $('#myroomname').val(`${window.location.origin}/?r=${room}`);
        document.getElementById("leaveRoomButton").disabled = false;
        $('#loginModal').modal('hide');
        $('#loader-complete').remove();
    });

    /* Received when a user leaves */
    connection.on("PlayerLeft", playerId => $(`#card-${playerId}`).remove());

    /* Received when a vote is cast */
    connection.on("PlayerVoted", playerId => {
        var id = `#status-${playerId}`;
        var nameplate = $(id);
        nameplate.addClass('bg-success');
        nameplate.addClass('border-success');
    });

    /* Received when a moderator instructs a reveal */
    connection.on("StartDiscussionPhase", gameResultInfo => {
        currentPhase = PHASE.Results;
        /* clear voteplate colours */
        var nameplates = $('.nameplate')
        nameplates.removeClass('bg-success');
        nameplates.removeClass('border-success');
        /* map the votes to their count and display */
        var votesNum = mapVotesToCountAndSetDisplay(gameResultInfo.votes);
        /* (re)draw the chart */
        var ctx = document.getElementById('resultsChart').getContext('2d');
        if (myChart === undefined) {
            myChart = createChart(ctx, votesNum);
        } else {
            myChart.data.datasets[0].data = votesNum;
            myChart.update();
        }
        /* switch to results tab */
        $('#results-tab').click();
    });

    /* Received when a moderator instructs a reset */
    connection.on("StartVotingPhase", () => {
        currentPhase = PHASE.Voting;
        /* clear voteplate */
        $('.voteplate').html('');
        /*  uncheck votes */
        $('.vote').removeClass('active');
        $('.vote').removeClass('focus');
        // add the red to observers
        $('.observer').addClass('bg-danger');
        $('.observer').addClass('border-danger');
        /* switch back to vote tab */
        $('#vote-tab').click();
    });

    /* Received when a player toggles the observer status */
    connection.on("PlayerChanged", playerChangeInfo => {
        var id = `#status-${playerChangeInfo.playerId}`;

        if (playerChangeInfo.isObserver) {
            $(id).removeClass('bg-success');
            $(id).removeClass('border-success');

            $(id).addClass('bg-danger');
            $(id).addClass('border-danger');
            $(id).addClass('observer');
        } else {
            // we need to know if voted, too.
            $(id).removeClass('bg-danger');
            $(id).removeClass('border-danger');
            $(id).removeClass('observer');

            if (playerChangeInfo.hasVoted) {
                $(id).addClass('bg-success');
                $(id).addClass('border-success');
            }
        }
    });

    /*
     * Configure SPA actions
     */

    /* Attempts to issue a vote. */
    $('.vote').click(function (event) {
        const value = $(this).attr('data-value');
        connection.invoke("SendVote", value).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });

    /* Issues a request for all participants to be shown the votes. */
    $('#revealVote').click(function (event) {
        connection.invoke("SendResult").catch(function (err) {
            return console.error(err.toString());
        });
    });

    /* Issues a request for all participants to reset their vote */
    $('#resetVote').click(function (event) {
        connection.invoke("BeginVoting").catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });

    /* Inform the server that this player is now an/not an observer */
    $('#playerIsObserver').change(function () {
        playerIsObserver = this.checked;
        connection.invoke("SendPlayerChange", playerIsObserver).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });

    $('#relogButton').click(function () {
        var url = `${window.location.protocol}//${window.location.host}/?r=${room}`;
        window.location = url;
    });
});
