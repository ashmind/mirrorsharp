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

    return {
      show: function(html, info, left, top, altBottom) {
        if (!this.active)
          ensureElement();

        element.innerHTML = html;
        element.style.transform = `translate(${left}px, ${top}px)`;
        if (!this.active) {
          element.removeAttribute("hidden");
          // Note: we have to show it *before* we check for a better position
          // otherwise we can't calculate the size
        }

        const rect = element.getBoundingClientRect();
        const betterLeft = (rect.right <= window.innerWidth) ? left : (left - (rect.right - window.innerWidth));
        const betterTop = (rect.bottom <= window.innerHeight) ? top : (altBottom - rect.height);
        if (betterLeft !== left || betterTop !== top)
          element.style.transform = `translate(${betterLeft}px, ${betterTop}px)`;

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
    delayedInteraction(this.CodeMirror, e.pageX, e.pageY);
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
    delayedInteraction(this.CodeMirror, e.touches[0].pageX, e.touches[0].pageY);
  }

  function click(e) {
    /* eslint-disable no-invalid-this */
    interaction(this.CodeMirror, e.pageX, e.pageY);
  }

  var activeTimeout;
  function delayedInteraction(cm, x, y) {
    /* eslint-disable no-invalid-this */
    if (activeTimeout) {
      clearTimeout(activeTimeout);
    }

    activeTimeout = setTimeout(function() {
      interaction(cm, x, y);
      activeTimeout = null;
    }, 100);
  }

  function interaction(cm, x, y) {
    var coords = cm.coordsChar({ left: x, top: y });
    if (tooltip.active && isInRange(coords, tooltip.info.range))
      return;

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
    showOrHide(cm, info);
  }

  function update(cm, info) {
    var coords = cm.coordsChar({ left: x, top: y });
    if (info && !isInRange(coords, info.range)) // mouse has moved before we got an async update
      return;
    showOrHide(cm, info);
  }

  function showOrHide(cm, info) {
    if (info == null) {
      tooltip.hide();
      return;
    }

    const showAt = cm.cursorCoords(CodeMirror.Pos(coords.line, info.range.from.ch));
    tooltip.show(info.html, info, showAt.left, showAt.bottom, showAt.top);
  }

  function isInRange(position, range) {
      if (position.line === range.from.line)
        return position.ch >= range.from.ch;
      if (position.line === range.to.line)
        return position.ch <= range.to.ch;
      return position.line > range.from.line
          && position.line < range.end.line
  }

  CodeMirror.defineExtension('infotipUpdate', update);

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
      update: function(info) { update(cm, info); }
    };
    cm.state.infotip = state;
    CodeMirror.on(wrapper, "click",      click);
    CodeMirror.on(wrapper, "touchstart", touchstart);
    CodeMirror.on(wrapper, "mousemove",  mousemove);
    CodeMirror.on(wrapper, "mouseout",   mouseout);
  });
});