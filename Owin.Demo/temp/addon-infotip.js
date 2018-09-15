(function(mod) {
  if (typeof exports === "object" && typeof module === "object") // CommonJS
    mod(require("codemirror"));
  else if (typeof define === "function" && define.amd) // AMD
    define(["codemirror"], mod);
  else // Plain browser env
    mod(window.CodeMirror);
})(function(CodeMirror) {
  "use strict";

  var tooltip = (function() {
    var element;
    var ensureElement = function() {
      if (element)
        return;
      element = document.createElement("div");
      element.className = "CodeMirror-infotip cm-s-default"; // TODO: dynamic theme based on current cm
      element.setAttribute("hidden", "hidden");
      CodeMirror.on(element, "click", function() { tooltip.hide(); });
      document.getElementsByTagName("body")[0].appendChild(element);
    };

    var clearElement = function() {
      while (element.firstChild) {
        element.removeChild(element.firstChild);
      }
    }

    var setContent = function(html) {
      if (html instanceof Array) {
        clearElement();
        for (var i = 0; i< html.length; i++) {
          element.appendChild(html[i]);
        }
      }
      else if (html instanceof HTMLElement) {
        clearElement();
        element.appendChild(html[i]);
      }
      else {
        element.innerHTML = html;
      }
    }

    return {
      show: function(html, info, left, top, altBottom) {
        if (!this.active)
          ensureElement();

        setContent(html);
        element.style.top = top + 'px';
        element.style.left = left + 'px';
        if (!this.active) {
          element.removeAttribute("hidden");
          // Note: we have to show it *before* we check for a better position
          // otherwise we can't calculate the size
        }

        const rect = element.getBoundingClientRect();
        const betterLeft = (rect.right <= window.innerWidth) ? left : (left - (rect.right - window.innerWidth));
        const betterTop = (rect.bottom <= window.innerHeight) ? top : (altBottom - rect.height);
        if (betterLeft !== left || betterTop !== top) {
            element.style.top = betterTop + 'px';
            element.style.left = betterLeft + 'px';
        }

        this.active = true;
        this.info = info;
      },

      hide: function() {
        if (!this.active || !element)
          return;
        element.setAttribute("hidden", "hidden");
        this.active = false;
      }
    };
  })();

  function mousemove(e) {
    /* eslint-disable no-invalid-this */
    updatePointer(this.CodeMirror, e.pageX, e.pageY);
    delayedInteraction(this.CodeMirror);
  }

  function mouseout(e) {
    /* eslint-disable no-invalid-this */
    var cm = this.CodeMirror;
    if (e.target !== cm.getWrapperElement())
      return;
    tooltip.hide();
  }

  function touchstart(e) {
    /* eslint-disable no-invalid-this */
    updatePointer(this.CodeMirror, e.touches[0].pageX, e.touches[0].pageY);
    delayedInteraction(this.CodeMirror);
  }

  function click(e) {
    /* eslint-disable no-invalid-this */
    updatePointer(this.CodeMirror, e.pageX, e.pageY);
    interaction(this.CodeMirror);
  }

  function updatePointer(cm, x, y) {
    const pointer = cm.state.infotip.pointer;
    pointer.x = x;
    pointer.y = y;
  }

  var activeTimeout;
  function delayedInteraction(cm) {
    /* eslint-disable no-invalid-this */
    if (activeTimeout) {
      clearTimeout(activeTimeout);
    }

    activeTimeout = setTimeout(function() {
      interaction(cm);
      activeTimeout = null;
    }, 100);
  }

  function interaction(cm) {
    var coords = getPointerCoords(cm);
    var getInfo = cm.state.infotip.getInfo || cm.getHelper(coords, "infotip");
    if (!getInfo)
      return;

    // var token = cm.getTokenAt(coords);
    // // this means that we are actually beyond the token, e.g.
    // // coordsChar() to the right of eol still returns last char
    // // on the line, but xRel will be 1 (to the right)
    // if (token.end === coords.ch && coords.xRel === 1) {
    //   tooltip.hide();
    //   return;
    // }
    // if (token === tooltip.token)
    //   return;

    // tooltip.token = token;
    var info = getInfo(cm, coords, cm.state.infotip.update);
    if (info !== undefined)
      showOrHide(cm, info);
  }

  function getPointerCoords(cm) {
    var pointer = cm.state.infotip.pointer;
    return cm.coordsChar({ left: pointer.x, top: pointer.y });
  }

  function update(cm, info) {
    var coords = getPointerCoords(cm);
    if (info && !isInRange(coords, info.range)) // mouse has moved before we got an async update
      return;

    showOrHide(cm, info);
  }

  function showOrHide(cm, info) {
    if (info == null) {
      tooltip.hide();
      return;
    }

    if (tooltip.active && rangesEqual(info.range, tooltip.info.range))
      return;

    const showAt = cm.cursorCoords(CodeMirror.Pos(info.range.from.line, info.range.from.ch));
    tooltip.show(info.html, info, showAt.left, showAt.bottom, showAt.top);
  }

  function rangesEqual(range, other) {
    return range.from.line === other.from.line
        && range.from.ch === other.from.ch
        && range.to.line === other.to.line
        && range.to.ch === other.to.ch;
  }

  function isInRange(position, range) {
    if (position.line === range.from.line)
      return position.ch >= range.from.ch;
    if (position.line === range.to.line)
      return position.ch <= range.to.ch;
    return position.line > range.from.line
        && position.line < range.to.line
  }

  CodeMirror.defineExtension('infotipUpdate', function(info) {
    /* eslint-disable no-invalid-this */
    update(this, info);
  });

  CodeMirror.defineExtension('infotipIsActive', function() {
    return tooltip.active;
  });

  CodeMirror.defineOption("infotip", null, function(cm, options, old) {
    var wrapper = cm.getWrapperElement();
    var state = cm.state.infotip;
    if (old && old !== CodeMirror.Init && state) {
      CodeMirror.off(wrapper, "click",      click);
      CodeMirror.off(wrapper, "touchstart", touchstart);
      CodeMirror.off(wrapper, "mousemove",  mousemove);
      CodeMirror.off(wrapper, "mouseout",   mouseout);
      delete cm.state.infotip;
    }

    if (!options)
      return;

    state = {
      getInfo: options.getInfo,
      update: function(info) { update(cm, info); },
      pointer: {}
    };
    cm.state.infotip = state;
    CodeMirror.on(wrapper, "click",      click);
    CodeMirror.on(wrapper, "touchstart", touchstart);
    CodeMirror.on(wrapper, "mousemove",  mousemove);
    CodeMirror.on(wrapper, "mouseout",   mouseout);
  });
});