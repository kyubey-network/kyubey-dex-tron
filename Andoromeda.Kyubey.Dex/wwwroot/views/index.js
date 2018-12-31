app = new Vue({
    router: router,
    data: {
        chainId: 'aca376f206b8fc25a6ed44dbdc66547c36c6c33e3a119ffbeaef943642f0e906',
        host: 'nodes.get-scatter.com',
        account: null,
        uuid: null,
        loginMode: null,
        eos: null,
        dexAccount: 'kyubeydex.bp',
        requiredFields: null,
        currentHost: location.protocol + "//" + location.host,
        volume: 0,
        signalr: {
            simplewallet: {
                connection: null,
                listeners: []
            }
        },
        qrcodeIsValid: true,
        qrcodeTimer: null,
        qrcodeExchange: {
            title: null,
            content: null,
            items: [],
            account: null
        },
        _width: null,
        mobile: {
            nav: 'home'
        },
        control: {
            apiLock: false,
            currentNotification: null,
            notifications: [],
            notificationLock: false,
        }
    },
    created: function () {
        var self = this;
        this.initSignalR();
        qv.get(`/api/v1/lang/${this.lang}/info/volume`, {}).then(res => {
            if (res.code === 200) {
                self.volume = res.data;
            }
        });
        $(document).ready(function () {
            self._width = window.innerWidth;
        });
    },
    mounted: function () {
        var self = this;

        if (self.isMobile()) {
            setTimeout(function () {
                if (typeof scatter == 'undefined') {
                    const $t = self.$t.bind(self);
                    alert($t('tip_use_dapp_browser'));
                    return;
                }
                self.scatterLogin();
            }, 1000);
        };

        self.$nextTick(() => {
            window.addEventListener('resize', () => {
                if (self._width >= 768 && window.innerWidth < 768 || window.innerWidth >= 768 && self._width < 768) {
                    window.location.reload();
                }
                self._width = window.innerWidth;
            });
        });
    },
    watch: {
    },
    methods: {
        showMobileLanguageSelector() {
            $('.language-selector').modal('show');
        },
        isMobile: function () {
            return this._width < 768;
        },
        getEosHexData: function (code, action, args, callback) {
            qv.get(`/api/v1/lang/${app.lang}/node/AbiJsonToBin/${code}/${action}`, {
                jsonargs: JSON.stringify(args)
            }).then(x => {
                callback(x);
            });
        },
        startQRCodeExchange(title, content, items) {
            app.qrcodeExchange.title = title;
            app.qrcodeExchange.content = content;
            app.qrcodeExchange.items = items;
            app.qrcodeExchange.account = app.account == null ? "" : app.account.name;

            //set qrcode timer
            app.qrcodeIsValid = true;
            clearTimeout(app.qrcodeTimer);
            app.qrcodeTimer = setTimeout(function () {
                app.qrcodeIsValid = false;
            }, 3 * 60 * 1000);

            $("#exchangeQRCodeBox").empty();

            var qrcode = new QRCode('exchangeQRCodeBox', {
                text: content,
                width: 200,
                height: 200,
                colorDark: "#000000",
                colorLight: "#ffffff",
                correctLevel: QRCode.CorrectLevel.L
            });

            $('#exchangeModal').modal('show');
        },
        getQRCodeActionObject(account, actionName, from, hexData, callbackUrl, desc) {
            var reqObj = {
                "protocol": "SimpleWallet",
                "version": "1.0",
                "action": "transaction",
                "blockchain": "eosio",
                "dappName": "Kyubey",
                "dappIcon": `${app.currentHost}/img/KYUBEY_logo.png`,
                "actions": [{ "code": account, "action": actionName, "binargs": hexData }],
                "from": from,
                "desc": desc,
                "expired": new Date().getTime() + (3 * 60 * 1000),
                "callback": callbackUrl
            };
            return reqObj;
        },
        startQRCodeFav(symbol, isFav) {
            var self = this;
            const $t = this.$t.bind(this);
            var args = { "symbol": symbol };
            var code = self.dexAccount;
            var action = isFav ? "addfav" : "removefav";
            app.getEosHexData(code, action, args, function (result) {
                app.startQRCodeExchange((isFav ? $t('title_add_fav', { symbol: symbol }) : $t('title_remove_fav', { symbol: symbol })), JSON.stringify(app.getQRCodeActionObject(code, action, app.account.name, result.data, `${self.currentHost}/api/v1/simplewallet/callback/action?actionType=${action}&uuid=${self.uuid}`)),
                    [
                        {
                            color: 'red',
                            text: $t('tip_use_medishares_app')
                        }
                    ]);
            });
        },
        startQRCodeCancelOrder(id, symbol, isBuy) {
            var self = this;
            const $t = this.$t.bind(this);
            var args = {
                "symbol": symbol,
                "id": id,
                "account": app.account.name
            };
            var code = self.dexAccount;
            var action = isBuy ? "cancelbuy" : "cancelsell";
            app.getEosHexData(code, action, args, function (result) {
                app.startQRCodeExchange($t('title_cancel_delegate', { symbol: symbol }), JSON.stringify(app.getQRCodeActionObject(code, action, app.account.name, result.data, `${self.currentHost}/api/v1/simplewallet/callback/action?actionType=cancel&uuid=${self.uuid}`)),
                    [
                        {
                            color: 'red',
                            text: $t('tip_use_medishares_app')
                        }
                    ]);
            });
        },
        _getLoginRequestObj: function (uuid) {
            var _this = this;
            var loginObj = {
                "protocol": "SimpleWallet",
                "version": "1.0",
                "dappName": "Kyubey",
                "dappIcon": `${_this.currentHost}/img/KYUBEY_logo.png`,
                "action": "login",
                "uuID": uuid,
                "loginUrl": `${_this.currentHost}/api/v1/simplewallet/callback/login`,
                "expired": new Date().getTime() + (3 * 60 * 1000),
                "loginMemo": "kyubey login"
            };
            return loginObj;
        },
        generateLoginQRCode: function (idSelector, uuid) {
            $("#" + idSelector).empty();
            var loginObj = this._getLoginRequestObj(uuid);
            var qrcode = new QRCode(idSelector, {
                text: JSON.stringify(loginObj),
                width: 200,
                height: 200,
                colorDark: "#000000",
                colorLight: "#ffffff",
                correctLevel: QRCode.CorrectLevel.L
            });
        },
        getSimpleWalletUUID: function () {
            return this.uuid;
        },
        generateUUID: function () {
            var s = [];
            var hexDigits = "0123456789abcdef";
            for (var i = 0; i < 36; i++) {
                s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
            }
            s[14] = "4";
            s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);
            s[8] = s[13] = s[18] = s[23] = "-";

            var uuid = s.join("");
            return uuid;
        },
        initSignalR: function () {
            var self = this;
            const $t = this.$t.bind(this);
            self.signalr.simplewallet.connection = new signalR.HubConnectionBuilder()
                .configureLogging(signalR.LogLevel.Trace)
                .withUrl('/signalr/simplewallet', {})
                .withHubProtocol(new signalR.JsonHubProtocol())
                .build();

            // TODO: Receiving some signals for updating query view.
            self.signalr.simplewallet.connection.on('simpleWalletLoginSucceeded', (account) => {
                self.account = {
                    name: account
                };
                self.loginMode = 'Simple Wallet';
                $('#loginModal').modal('hide');
            });

            self.signalr.simplewallet.connection.on('simpleWalletExchangeSucceeded', () => {
                $('#exchangeModal').modal('hide');
                app.notification("succeeded", $t('delegate_succeed'));

                var current = LazyRouting.GetCurrentComponent();
                if (current && current.delegateCallBack) {
                    current.delegateCallBack();
                }
            });

            self.signalr.simplewallet.connection.on('simpleWalletCancelSucceeded', () => {
                $('#exchangeModal').modal('hide');
                app.notification("succeeded", $t('tip_cancel_succeed'));

                var current = LazyRouting.GetCurrentComponent();
                if (current && current.cancelCallBack) {
                    current.cancelCallBack();
                }
            });

            self.signalr.simplewallet.connection.on('simpleWalletDoFavSucceeded', () => {
                $('#exchangeModal').modal('hide');
                app.notification("succeeded", $t('tip_fav_succeed'));

                var current = LazyRouting.GetCurrentComponent();
                if (current && current.doFavCallBack) {
                    current.doFavCallBack();
                }
            });

            self.signalr.simplewallet.connection.on('simpleWalletRemoveFavSucceeded', () => {
                $('#exchangeModal').modal('hide');
                app.notification("succeeded", $t('tip_cancel_succeed'));

                var current = LazyRouting.GetCurrentComponent();
                if (current && current.doFavCallBack) {
                    current.doFavCallBack();
                }
            });

            self.signalr.simplewallet.connection.start().then(function () {
                self.uuid = self.generateUUID();
                return self.signalr.simplewallet.connection.invoke('bindUUID', self.uuid);
            });

            self.signalr.simplewallet.connection.onclose(async () => {
                await self.restartSignalR();
            });
        },
        restartSignalR: async function () {
            var self = this;
            try {
                await self.signalr.simplewallet.connection.start();
                self.signalr.simplewallet.connection.invoke('bindUUID', self.uuid);
                console.log('reconnected');
            } catch (err) {
                console.warn(err);
                if (err.statusCode > 500 || err.statusCode == 0) {
                    setTimeout(() => self.restartSignalR(), 2000);
                }
            }
        },
        login: function () {
            $('#loginModal').modal('show');
            this.refreshLoginQRCode();
        },
        refreshLoginQRCode: function () {
            var _this = this;
            this.generateLoginQRCode("loginQRCode", this.getSimpleWalletUUID());
            this.qrcodeIsValid = true;
            clearTimeout(_this.qrcodeTimer);
            _this.qrcodeTimer = setTimeout(function () {
                _this.qrcodeIsValid = false;
            }, 3 * 60 * 1000);
        },
        scatterLogin: function () {
            const $t = this.$t.bind(this);
            if (!('scatter' in window)) {
                showModal($t('scatter_not_found'), $t('scatter_tip'));
            } else {
                var self = this;
                var network = {
                    blockchain: 'eos',
                    host: self.host,
                    port: 443,
                    protocol: 'https',
                    chainId: self.chainId
                };
                scatter.getIdentity({ accounts: [network] }).then(identity => {
                    self.account = identity.accounts.find(acc => acc.blockchain === 'eos');
                    self.loginMode = 'Scatter Addons';
                    self.eos = scatter.eos(network, Eos, {});
                    self.requiredFields = { accounts: [network] };
                });
            }
            $('#loginModal').modal('hide');
        },
        scatterLogout: function () {
            var self = this;
            if (self.loginMode && (self.loginMode === 'Scatter Addons' || self.loginMode === 'Scatter Desktop')) {
                scatter.forgetIdentity()
                    .then(() => {
                        self.account = null;
                        self.loginMode = null;
                    });
            } else {
                self.account = null;
                self.loginMode = null;
            }
        },
        switchAccount: function () {
            self = this;
            if (self.loginMode == null) return;
            if (self.loginMode === 'Simple Wallet') {
                account = null;
            } else {
                if (self.loginMode === 'Scatter Addons' || self.loginMode === 'Scatter Desktop') {
                    this.scatterLogout();
                }
            }
            this.login();
        },
        redirect: function (name, path, params, query) {
            if (name && !path)
                path = name;
            LazyRouting.RedirectTo(name, path, params, query);
        },
        setLang: function (param) {
            this.$i18n.locale = param;
        },
        marked: function (md) {
            return marked(md);
        },
        notification: function (level, title, detail, button) {
            var item = { level: level, title: title, detail: detail };
            if (level === 'important') {
                item.button = button;
            }
            this.control.notifications.push(item);
            if (this.control.currentNotification && this.control.currentNotification.level === 'pending') {
                this.control.notificationLock = false;
            }
            this._showNotification(level === 'important' ? true : false);
        },
        clickNotification: function () {
            this._releaseNotification();
        },
        _showNotification: function (manualRelease) {
            var self = this;
            if (!this.control.notificationLock && this.control.notifications.length) {
                this.control.notificationLock = true;
                var notification = this.control.notifications[0];
                this.control.notifications = this.control.notifications.slice(1);
                this.control.currentNotification = notification;
                if (!manualRelease) {
                    setTimeout(function () {
                        self._releaseNotification();
                    }, 5000);
                }
            }
        },
        _releaseNotification: function () {
            var self = this;
            self.control.currentNotification = null;
            setTimeout(function () {
                self.control.notificationLock = false;
                if (self.control.notifications.length) {
                    self._showNotification();
                }
            }, 250);
        },
        // token, is add favorite, callback
        toggleFav(token, isAdd, cb) {
            if (this.loginMode === 'Simple Wallet') {
                this.startQRCodeFav(token, isAdd);
            } else if (this.loginMode === 'Scatter Addons' || this.loginMode === 'Scatter Desktop') {
                this.scatterFav(token, isAdd, cb);
            }
        },
        scatterFav(token, isAdd, cb) {
            const { account, requiredFields, eos } = app;
            const $t = this.$t.bind(this);
            eos.contract('kyubeydex.bp', { requiredFields })
                .then(contract => {
                    if (isAdd) {
                        return contract.addfav(
                            token,
                            {
                                authorization: [`${account.name}@${account.authority}`]
                            });
                    } else {
                        return contract.removefav(
                            token,
                            {
                                authorization: [`${account.name}@${account.authority}`]
                            });
                    }
                })
                .then(cb)
                .catch(error => {
                    showModal($t('collection_fail'), error.message + $t('Please contact us if you have any questions'));
                });
        }
    },
    computed: {
        isSignedIn: function () {
            return !(app.account == null || app.account.name == null);
        },
        lang: function () {
            return this.$i18n.locale;
        }
    },
    i18n: i18n
});
