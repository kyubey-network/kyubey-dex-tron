const ecc = require('eosjs-ecc')
module.exports = {
    verify: function (callback, signature, data, pubkey) {
        try {
            var result = ecc.verify(signature, data, pubkey);
            callback(null, result);
        }
        catch (err) {
            callback(err);
        }
    }
};