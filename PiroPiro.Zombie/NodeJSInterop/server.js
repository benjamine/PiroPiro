var net = require('net');
var util = require('util');
var Browser = require('zombie');
var buffer = "";
var __sCount = 0;
var __s = {};

var port = 8124;

net.createServer(function (stream) {
  stream.setEncoding('utf8');
  stream.allowHalfOpen = true;

  stream.on('data', function (data) {
  
	// separate messages by empty line
	var eor = data.indexOf('\n\n');
	if (eor >=0) {
	
		// '\n' separates requests
		var req = JSON.parse(buffer + data.substr(0,eor));
		
		console.log('  [REQUEST]');
		console.log(req);
		
		var res = {};
		var sendResponse = function() {
			res.resultType = typeof(res.result);
			
			// separate messages by empty line
			try {
				var jsonResponse = JSON.stringify(res);
			} catch(err){
				// serialization error (eg. circular structure)
				res = { sid: res.sid, error: err+'', serializationError: true };
				jsonResponse = JSON.stringify(res);
				util.puts(err.stack);
			}
			console.log('  [RESPONSE]');
			console.log(res);
			stream.write(jsonResponse+'\n\n');
		}

		if (!req.sid){
			// create a new session
			__sCount++;
			res.sid = __sCount+'';
			__s[res.sid] = { id: res.sid, vars: {} };
			res.result = res.sid;
		} else {
		
			if (!__s[req.sid]){
				req.session = __s[req.sid] = { id: req.sid, vars: {} };
			} else {
				req.session = __s[req.sid];
			}
			res.sid = req.sid;
		
			var done = function(err) {
				if (err){
					res.error = err+'';
					util.puts(err.stack);
				} else {
					var args = Array.prototype.slice.call(arguments, 1);
					for (var i =0; i < args.length; i++) {
						if (typeof args[i] == 'object') {
							try {
								JSON.stringify(args[i]);
							} catch(err) {
								// cannot serialize this arg (eg. circular reference), remove from results
								args[i] = null;
							}
						}
					}
					if (args.length > 1){
						res.result = args[1];
					}else{
						res.result = args;
					}
				}
				sendResponse();
			}			

			var doneNoResult = function(err) {
				res.result = null;
				if (err){
					res.error = err+'';
					util.puts(err.stack);
				}
				sendResponse();
			}			
			
			var _ref = function(variable){
				// assign reference ids to variables
				if (Array.isArray(variable)) {
					var refarray = [];
					for (var i=0; i< variable.length; i++){
						refarray.push(_ref(variable[i]));
					}
					return refarray.join(',');
				} else {
					if (typeof req.session.nextId == 'undefined'){
						req.session.nextId = 1;
					}
					while (typeof req.session.vars[req.session.nextId+''] != 'undefined'){
						req.session.nextId++;
					}
					var rid = req.session.nextId+'';
					req.session.vars[rid] = variable;
					req.session.nextId++;
					return rid;
				}
			};
			
			var evalResult;
			
			try {
				res.result = eval('(function(){'+req.code+'})()');
			} catch(err) {
				res.error = err+'';
				util.puts(err.stack);
			}
		}		
		
		if (!req.async || res.error){
			sendResponse();
		} else {
			// async callback, expect a call to done()
		}
		buffer = data.substr(eor+2);
	} else {
		buffer += data;
	}
  });

  stream.on('end', function () {
    buffer = "";
  });
}).listen(port, 'localhost');

console.log('Server running at http://localhost:'+port+'/');
