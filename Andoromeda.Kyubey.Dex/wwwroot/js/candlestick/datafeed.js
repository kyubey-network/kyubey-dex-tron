function FeedBase() { }

FeedBase.prototype.onReady = function (callback) {
    callback(this._configuration)
}

FeedBase.prototype.getSendSymbolName = function (symbolName) {
    var name = symbolName.split('/')
    return (name[0] + name[1]).toLocaleLowerCase()
}

FeedBase.prototype.resolveSymbol = function (symbolName, onResolve, onError) {
    onResolve({
        "name": symbolName + "/EOS",
        "timezone": "Asia/Shanghai",
        "pricescale": 100000000,
        "minmov": 1,
        "minmov2": 0,
        "ticker": symbolName + "/EOS",
        "description": "",
        "session": "24x7",
        "type": "bitcoin",
        "volume_precision": 10,
        "has_intraday": true,
        "intraday_multipliers": ['1', '3', '5', '15', '30', '60', '240', '360', '1440'],
        "has_weekly_and_monthly": false,
        "has_no_volume": false,
        "regular_session": "24x7"
    })
}

FeedBase.prototype.getApiTime = function (resolution) {
    switch (resolution) {
        case '1':
            return 'M1'
        case '3':
            return 'M3'
        case '5':
            return 'M5'
        case '15':
            return 'M15'
        case '30':
            return 'M30'
        case '60':
            return 'H1'
        case '240':
            return 'H4'
        case '360':
            return 'H6'
        case '1D':
            return 'D1'
        default:
            return 'M1'
    }
}

FeedBase.prototype.subscribeBars = function (symbolInfo, resolution, onTick, listenerGuid, onResetCacheNeededCallback) {
}

FeedBase.prototype.unsubscribeBars = function (listenerGuid) {
}