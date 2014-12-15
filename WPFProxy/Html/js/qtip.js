function refreshTips() {
    $('.owe-tips[title]').qtip({
        content: {
            text: false
        },
        position: {
            my: 'right middle',
            at: 'left center'
        },
        style: {
            tip: true,
            classes: 'ui-tooltip-owe-tips'
        },
        show: {
            delay: 1000
        }
    });

    
	$('.tip_help[title]').qtip({
	content: {
	text: false
	},
	position: {
	my: 'bottom left',
	at: 'top middle'
	},
	style: {
	tip: true,
	classes: 'ui-tooltip-owe-help'
	}
	});
}

$(document).ready(function () { refreshTips(); });
