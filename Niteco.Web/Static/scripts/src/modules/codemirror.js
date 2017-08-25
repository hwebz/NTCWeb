// All functions that need access to the editor's state live inside
// the CodeMirror function. Below that, at the bottom of the file,
// some utilities are defined.

// CodeMirror is the only global var we claim
var CodeMirror = (function() {
  // This is the function that produces an editor instance. It's
  // closure is used to store the editor state.
  function CodeMirror(place, givenOptions) {
    // Determine effective options based on given values and defaults.
    var options = {}, defaults = CodeMirror.defaults;
    for (var opt in defaults)
      if (defaults.hasOwnProperty(opt))
        options[opt] = (givenOptions && givenOptions.hasOwnProperty(opt) ? givenOptions : defaults)[opt];

    var targetDocument = options["document"];
    // The element in which the editor lives.
    var wrapper = targetDocument.createElement("div");
    wrapper.className = "CodeMirror";
    // This mess creates the base DOM structure for the editor.
    wrapper.innerHTML =
      '<div style="overflow: hidden; position: relative; width: 1px; height: 0px;">' + // Wraps and hides input textarea
        '<textarea style="position: absolute; width: 10000px;" wrap="off" ' +
          'autocorrect="off" autocapitalize="off"></textarea></div>' +
      '<div class="CodeMirror-scroll cm-s-' + options.theme + '">' +
        '<div style="position: relative">' + // Set to the height of the text, causes scrolling
          '<div style="position: absolute; height: 0; width: 0; overflow: hidden;"></div>' +
          '<div style="position: relative">' + // Moved around its parent to cover visible view
            '<div class="CodeMirror-gutter"><div class="CodeMirror-gutter-text"></div></div>' +
            // Provides positioning relative to (visible) text origin
            '<div class="CodeMirror-lines"><div style="position: relative" draggable="true">' +
              '<pre class="CodeMirror-cursor">&#160;</pre>' + // Absolutely positioned blinky cursor
              '<div></div>' + // This DIV contains the actual code
            '</div></div></div></div></div>';
    if (place.appendChild) place.appendChild(wrapper); else place(wrapper);
    // I've never seen more elegant code in my life.
    var inputDiv = wrapper.firstChild, input = inputDiv.firstChild,
        scroller = wrapper.lastChild, code = scroller.firstChild,
        measure = code.firstChild, mover = measure.nextSibling,
        gutter = mover.firstChild, gutterText = gutter.firstChild,
        lineSpace = gutter.nextSibling.firstChild,
        cursor = lineSpace.firstChild, lineDiv = cursor.nextSibling;
    if (options.tabindex != null) input.tabindex = options.tabindex;
    if (!options.gutter && !options.lineNumbers) gutter.style.display = "none";

    // Check for problem with IE innerHTML not working when we have a
    // P (or similar) parent node.
    try { stringWidth("x"); }
    catch (e) {
      if (e.message.match(/unknown runtime/i))
        e = new Error("A CodeMirror inside a P-style element does not work in Internet Explorer. (innerHTML bug)");
      throw e;
    }

    // Delayed object wrap timeouts, making sure only one is active. blinker holds an interval.
    var poll = new Delayed(), highlight = new Delayed(), blinker;

    // mode holds a mode API object. lines an array of Line objects
    // (see Line constructor), work an array of lines that should be
    // parsed, and history the undo history (instance of History
    // constructor).
    var mode, lines = [new Line("")], work, focused;
    loadMode();
    // The selection. These are always maintained to point at valid
    // positions. Inverted is used to remember that the user is
    // selecting bottom-to-top.
    var sel = {from: {line: 0, ch: 0}, to: {line: 0, ch: 0}, inverted: false};
    // Selection-related flags. shiftSelecting obviously tracks
    // whether the user is holding shift. reducedSelection is a hack
    // to get around the fact that we can't create inverted
    // selections. See below.
    var shiftSelecting, reducedSelection, lastClick, lastDoubleClick, draggingText;
    // Variables used by startOperation/endOperation to track what
    // happened during the operation.
    var updateInput, changes, textChanged, selectionChanged, leaveInputAlone, gutterDirty;
    // Current visible range (may be bigger than the view window).
    var showingFrom = 0, showingTo = 0, lastHeight = 0, curKeyId = null;
    // editing will hold an object describing the things we put in the
    // textarea, to help figure out whether something changed.
    // bracketHighlighted is used to remember that a backet has been
    // marked.
    var editing, bracketHighlighted;
    // Tracks the maximum line length so that the horizontal scrollbar
    // can be kept static when scrolling.
    var maxLine = "", maxWidth;

    // Initialize the content.
    operation(function(){setValue(options.value || ""); updateInput = false;})();
    var history = new History();

    // Register our event handlers.
    connect(scroller, "mousedown", operation(onMouseDown));
    connect(scroller, "dblclick", operation(onDoubleClick));
    connect(lineSpace, "dragstart", onDragStart);
    // Gecko browsers fire contextmenu *after* opening the menu, at
    // which point we can't mess with it anymore. Context menu is
    // handled in onMouseDown for Gecko.
    if (!gecko) connect(scroller, "contextmenu", onContextMenu);
    connect(scroller, "scroll", function() {
      updateDisplay([]);
      if (options.fixedGutter) gutter.style.left = scroller.scrollLeft + "px";
      if (options.onScroll) options.onScroll(instance);
    });
    connect(window, "resize", function() {updateDisplay(true);});
    connect(input, "keyup", operation(onKeyUp));
    connect(input, "input", function() {fastPoll(curKeyId);});
    connect(input, "keydown", operation(onKeyDown));
    connect(input, "keypress", operation(onKeyPress));
    connect(input, "focus", onFocus);
    connect(input, "blur", onBlur);

    connect(scroller, "dragenter", e_stop);
    connect(scroller, "dragover", e_stop);
    connect(scroller, "drop", operation(onDrop));
    connect(scroller, "paste", function(){focusInput(); fastPoll();});
    connect(input, "paste", function(){fastPoll();});
    connect(input, "cut", function(){fastPoll();});

    // IE throws unspecified error in certain cases, when
    // trying to access activeElement before onload
    var hasFocus; try { hasFocus = (targetDocument.activeElement == input); } catch(e) { }
    if (hasFocus) setTimeout(onFocus, 20);
    else onBlur();

    function isLine(l) {return l >= 0 && l < lines.length;}
    // The instance object that we'll return. Mostly calls out to
    // local functions in the CodeMirror function. Some do some extra
    // range checking and/or clipping. operation is used to wrap the
    // call so that changes it makes are tracked, and the display is
    // updated afterwards.
    var instance = wrapper.CodeMirror = {
      getValue: getValue,
      setValue: operation(setValue),
      getSelection: getSelection,
      replaceSelection: operation(replaceSelection),
      focus: function(){focusInput(); onFocus(); fastPoll();},
      setOption: function(option, value) {
        options[option] = value;
        if (option == "lineNumbers" || option == "gutter" || option == "firstLineNumber")
          operation(gutterChanged)();
        else if (option == "mode" || option == "indentUnit") loadMode();
        else if (option == "readOnly" && value == "nocursor") input.blur();
        else if (option == "theme") scroller.className = scroller.className.replace(/cm-s-\w+/, "cm-s-" + value);
      },
      getOption: function(option) {return options[option];},
      undo: operation(undo),
      redo: operation(redo),
      indentLine: operation(function(n, dir) {
        if (isLine(n)) indentLine(n, dir == null ? "smart" : dir ? "add" : "subtract");
      }),
      historySize: function() {return {undo: history.done.length, redo: history.undone.length};},
      clearHistory: function() {history = new History();},
      matchBrackets: operation(function(){matchBrackets(true);}),
      getTokenAt: function(pos) {
        pos = clipPos(pos);
        return lines[pos.line].getTokenAt(mode, getStateBefore(pos.line), pos.ch);
      },
      getStateAfter: function(line) {
        line = clipLine(line == null ? lines.length - 1: line);
        return getStateBefore(line + 1);
      },
      cursorCoords: function(start){
        if (start == null) start = sel.inverted;
        return pageCoords(start ? sel.from : sel.to);
      },
      charCoords: function(pos){return pageCoords(clipPos(pos));},
      coordsChar: function(coords) {
        var off = eltOffset(lineSpace);
        var line = clipLine(Math.min(lines.length - 1, showingFrom + Math.floor((coords.y - off.top) / lineHeight())));
        return clipPos({line: line, ch: charFromX(clipLine(line), coords.x - off.left)});
      },
      getSearchCursor: function(query, pos, caseFold) {return new SearchCursor(query, pos, caseFold);},
      markText: operation(markText),
      setMarker: operation(addGutterMarker),
      clearMarker: operation(removeGutterMarker),
      setLineClass: operation(setLineClass),
      lineInfo: lineInfo,
      addWidget: function(pos, node, scroll, vert, horiz) {
        pos = localCoords(clipPos(pos));
        var top = pos.yBot, left = pos.x;
        node.style.position = "absolute";
        code.appendChild(node);
        if (vert == "over") top = pos.y;
        else if (vert == "near") {
          var vspace = Math.max(scroller.offsetHeight, lines.length * lineHeight()),
              hspace = Math.max(code.clientWidth, lineSpace.clientWidth) - paddingLeft();
          if (pos.yBot + node.offsetHeight > vspace && pos.y > node.offsetHeight)
            top = pos.y - node.offsetHeight;
          if (left + node.offsetWidth > hspace)
            left = hspace - node.offsetWidth;
        }
        node.style.top = (top + paddingTop()) + "px";
        node.style.left = node.style.right = "";
        if (horiz == "right") {
          left = code.clientWidth - node.offsetWidth;
          node.style.right = "0px";
        } else {
          if (horiz == "left") left = 0;
          else if (horiz == "middle") left = (code.clientWidth - node.offsetWidth) / 2;
          node.style.left = (left + paddingLeft()) + "px";
        }
        if (scroll)
          scrollIntoView(left, top, left + node.offsetWidth, top + node.offsetHeight);
      },

      lineCount: function() {return lines.length;},
      getCursor: function(start) {
        if (start == null) start = sel.inverted;
        return copyPos(start ? sel.from : sel.to);
      },
      somethingSelected: function() {return !posEq(sel.from, sel.to);},
      setCursor: operation(function(line, ch) {
        if (ch == null && typeof line.line == "number") setCursor(line.line, line.ch);
        else setCursor(line, ch);
      }),
      setSelection: operation(function(from, to) {setSelection(clipPos(from), clipPos(to || from));}),
      getLine: function(line) {if (isLine(line)) return lines[line].text;},
      setLine: operation(function(line, text) {
        if (isLine(line)) replaceRange(text, {line: line, ch: 0}, {line: line, ch: lines[line].text.length});
      }),
      removeLine: operation(function(line) {
        if (isLine(line)) replaceRange("", {line: line, ch: 0}, clipPos({line: line+1, ch: 0}));
      }),
      replaceRange: operation(replaceRange),
      getRange: function(from, to) {return getRange(clipPos(from), clipPos(to));},

      coordsFromIndex: function(index) {        
        var total = lines.length, pos = 0, line, ch, len;
        
        for (line = 0; line < total; line++) {
          len = lines[line].text.length + 1;
          if (pos + len > index) { ch = index - pos; break; }
          pos += len;
        }
        return clipPos({line: line, ch: ch});
      },

      operation: function(f){return operation(f)();},
      refresh: function(){updateDisplay(true);},
      getInputField: function(){return input;},
      getWrapperElement: function(){return wrapper;},
      getScrollerElement: function(){return scroller;},
      getGutterElement: function(){return gutter;}
    };

    function setValue(code) {
      var top = {line: 0, ch: 0};
      updateLines(top, {line: lines.length - 1, ch: lines[lines.length-1].text.length},
                  splitLines(code), top, top);
      updateInput = true;
    }
    function getValue(code) {
      var text = [];
      for (var i = 0, l = lines.length; i < l; ++i)
        text.push(lines[i].text);
      return text.join("\n");
    }

    function onMouseDown(e) {
      // Check whether this is a click in a widget
      for (var n = e_target(e); n != wrapper; n = n.parentNode)
        if (n.parentNode == code && n != mover) return;

      // First, see if this is a click in the gutter
      for (var n = e_target(e); n != wrapper; n = n.parentNode)
        if (n.parentNode == gutterText) {
          if (options.onGutterClick)
            options.onGutterClick(instance, indexOf(gutterText.childNodes, n) + showingFrom, e);
          return e_preventDefault(e);
        }

      var start = posFromMouse(e);

      switch (e_button(e)) {
      case 3:
        if (gecko && !mac) onContextMenu(e);
        return;
      case 2:
        if (start) setCursor(start.line, start.ch, true);
        return;
      }
      // For button 1, if it was clicked inside the editor
      // (posFromMouse returning non-null), we have to adjust the
      // selection.
      if (!start) {if (e_target(e) == scroller) e_preventDefault(e); return;}

      if (!focused) onFocus();

      var now = +new Date;
      if (lastDoubleClick > now - 400) {
        e_preventDefault(e);
        return selectLine(start.line);
      } else if (lastClick > now - 400) {
        lastDoubleClick = now;
        e_preventDefault(e);
        return selectWordAt(start);
      } else { lastClick = now; }

      var last = start, going;
      if (dragAndDrop && !posEq(sel.from, sel.to) &&
          !posLess(start, sel.from) && !posLess(sel.to, start)) {
        // Let the drag handler handle this.
        var up = connect(targetDocument, "mouseup", operation(function(e2) {
          draggingText = false;
          up();
          if (Math.abs(e.clientX - e2.clientX) + Math.abs(e.clientY - e2.clientY) < 10) {
            e_preventDefault(e2);
            setCursor(start.line, start.ch, true);
            focusInput();
          }
        }), true);
        draggingText = true;
        return;
      }
      e_preventDefault(e);
      setCursor(start.line, start.ch, true);

      function extend(e) {
        var cur = posFromMouse(e, true);
        if (cur && !posEq(cur, last)) {
          if (!focused) onFocus();
          last = cur;
          setSelectionUser(start, cur);
          updateInput = false;
          var visible = visibleLines();
          if (cur.line >= visible.to || cur.line < visible.from)
            going = setTimeout(operation(function(){extend(e);}), 150);
        }
      }

      var move = connect(targetDocument, "mousemove", operation(function(e) {
        clearTimeout(going);
        e_preventDefault(e);
        extend(e);
      }), true);
      var up = connect(targetDocument, "mouseup", operation(function(e) {
        clearTimeout(going);
        var cur = posFromMouse(e);
        if (cur) setSelectionUser(start, cur);
        e_preventDefault(e);
        focusInput();
        updateInput = true;
        move(); up();
      }), true);
    }
    function onDoubleClick(e) {
      var start = posFromMouse(e);
      if (!start) return;
      lastDoubleClick = +new Date;
      e_preventDefault(e);
      selectWordAt(start);
    }
    function onDrop(e) {
      e.preventDefault();
      var pos = posFromMouse(e, true), files = e.dataTransfer.files;
      if (!pos || options.readOnly) return;
      if (files && files.length && window.FileReader && window.File) {
        function loadFile(file, i) {
          var reader = new FileReader;
          reader.onload = function() {
            text[i] = reader.result;
            if (++read == n) {
	      pos = clipPos(pos);
	      var end = replaceRange(text.join(""), pos, pos);
	      setSelectionUser(pos, end);
	    }
          };
          reader.readAsText(file);
        }
        var n = files.length, text = Array(n), read = 0;
        for (var i = 0; i < n; ++i) loadFile(files[i], i);
      }
      else {
        try {
          var text = e.dataTransfer.getData("Text");
          if (text) {
	    var end = replaceRange(text, pos, pos);
	    var curFrom = sel.from, curTo = sel.to;
	    setSelectionUser(pos, end);
            if (draggingText) replaceRange("", curFrom, curTo);
	    focusInput();
	  }
        }
        catch(e){}
      }
    }
    function onDragStart(e) {
      var txt = getSelection();
      // This will reset escapeElement
      htmlEscape(txt);
      e.dataTransfer.setDragImage(escapeElement, 0, 0);
      e.dataTransfer.setData("Text", txt);
    }
    function onKeyDown(e) {
      if (!focused) onFocus();

      var code = e.keyCode;
      // IE does strange things with escape.
      if (ie && code == 27) { e.returnValue = false; }
      // Tries to detect ctrl on non-mac, cmd on mac.
      var mod = (mac ? e.metaKey : e.ctrlKey) && !e.altKey, anyMod = e.ctrlKey || e.altKey || e.metaKey;
      if (code == 16 || e.shiftKey) shiftSelecting = shiftSelecting || (sel.inverted ? sel.to : sel.from);
      else shiftSelecting = null;
      // First give onKeyEvent option a chance to handle this.
      if (options.onKeyEvent && options.onKeyEvent(instance, addStop(e))) return;

      if (code == 33 || code == 34) {scrollPage(code == 34); return e_preventDefault(e);} // page up/down
      if (mod && ((code == 36 || code == 35) || // ctrl-home/end
                  mac && (code == 38 || code == 40))) { // cmd-up/down
        scrollEnd(code == 36 || code == 38); return e_preventDefault(e);
      }
      if (mod && code == 65) {selectAll(); return e_preventDefault(e);} // ctrl-a
      if (!options.readOnly) {
        if (!anyMod && code == 13) {return;} // enter
        if (!anyMod && code == 9 && handleTab(e.shiftKey)) return e_preventDefault(e); // tab
        if (mod && code == 90) {undo(); return e_preventDefault(e);} // ctrl-z
        if (mod && ((e.shiftKey && code == 90) || code == 89)) {redo(); return e_preventDefault(e);} // ctrl-shift-z, ctrl-y
      }
      if (code == 36) { if (options.smartHome) { smartHome(); return e_preventDefault(e); } }

      // Key id to use in the movementKeys map. We also pass it to
      // fastPoll in order to 'self learn'. We need this because
      // reducedSelection, the hack where we collapse the selection to
      // its start when it is inverted and a movement key is pressed
      // (and later restore it again), shouldn't be used for
      // non-movement keys.
      curKeyId = (mod ? "c" : "") + (e.altKey ? "a" : "") + code;
      if (sel.inverted && movementKeys[curKeyId] === true) {
        var range = selRange(input);
        if (range) {
          reducedSelection = {anchor: range.start};
          setSelRange(input, range.start, range.start);
        }
      }
      // Don't save the key as a movementkey unless it had a modifier
      if (!mod && !e.altKey) curKeyId = null;
      fastPoll(curKeyId);
    }
    function onKeyUp(e) {
      if (options.onKeyEvent && options.onKeyEvent(instance, addStop(e))) return;
      if (reducedSelection) {
        reducedSelection = null;
        updateInput = true;
      }
      if (e.keyCode == 16) shiftSelecting = null;
    }
    function onKeyPress(e) {
      if (options.onKeyEvent && options.onKeyEvent(instance, addStop(e))) return;
      if (options.electricChars && mode.electricChars) {
        var ch = String.fromCharCode(e.charCode == null ? e.keyCode : e.charCode);
        if (mode.electricChars.indexOf(ch) > -1)
          setTimeout(operation(function() {indentLine(sel.to.line, "smart");}), 50);
      }
      var code = e.keyCode;
      // Re-stop tab and enter. Necessary on some browsers.
      if (code == 13) {if (!options.readOnly) handleEnter(); e_preventDefault(e);}
      else if (!e.ctrlKey && !e.altKey && !e.metaKey && code == 9 && options.tabMode != "default") e_preventDefault(e);
      else fastPoll(curKeyId);
    }

    function onFocus() {
      if (options.readOnly == "nocursor") return;
      if (!focused) {
        if (options.onFocus) options.onFocus(instance);
        focused = true;
        if (wrapper.className.search(/\bCodeMirror-focused\b/) == -1)
          wrapper.className += " CodeMirror-focused";
        if (!leaveInputAlone) prepareInput();
      }
      slowPoll();
      restartBlink();
    }
    function onBlur() {
      if (focused) {
        if (options.onBlur) options.onBlur(instance);
        focused = false;
        wrapper.className = wrapper.className.replace(" CodeMirror-focused", "");
      }
      clearInterval(blinker);
      setTimeout(function() {if (!focused) shiftSelecting = null;}, 150);
    }

    // Replace the range from from to to by the strings in newText.
    // Afterwards, set the selection to selFrom, selTo.
    function updateLines(from, to, newText, selFrom, selTo) {
      if (history) {
        var old = [];
        for (var i = from.line, e = to.line + 1; i < e; ++i) old.push(lines[i].text);
        history.addChange(from.line, newText.length, old);
        while (history.done.length > options.undoDepth) history.done.shift();
      }
      updateLinesNoUndo(from, to, newText, selFrom, selTo);
    }
    function unredoHelper(from, to) {
      var change = from.pop();
      if (change) {
        var replaced = [], end = change.start + change.added;
        for (var i = change.start; i < end; ++i) replaced.push(lines[i].text);
        to.push({start: change.start, added: change.old.length, old: replaced});
        var pos = clipPos({line: change.start + change.old.length - 1,
                           ch: editEnd(replaced[replaced.length-1], change.old[change.old.length-1])});
        updateLinesNoUndo({line: change.start, ch: 0}, {line: end - 1, ch: lines[end-1].text.length}, change.old, pos, pos);
        updateInput = true;
      }
    }
    function undo() {unredoHelper(history.done, history.undone);}
    function redo() {unredoHelper(history.undone, history.done);}

    function updateLinesNoUndo(from, to, newText, selFrom, selTo) {
      var recomputeMaxLength = false, maxLineLength = maxLine.length;
      for (var i = from.line; i <= to.line; ++i) {
        if (lines[i].text.length == maxLineLength) {recomputeMaxLength = true; break;}
      }

      var nlines = to.line - from.line, firstLine = lines[from.line], lastLine = lines[to.line];
      // First adjust the line structure, taking some care to leave highlighting intact.
      if (firstLine == lastLine) {
        if (newText.length == 1)
          firstLine.replace(from.ch, to.ch, newText[0]);
        else {
          lastLine = firstLine.split(to.ch, newText[newText.length-1]);
          var spliceargs = [from.line + 1, nlines];
          firstLine.replace(from.ch, null, newText[0]);
          for (var i = 1, e = newText.length - 1; i < e; ++i)
            spliceargs.push(Line.inheritMarks(newText[i], firstLine));
          spliceargs.push(lastLine);
          lines.splice.apply(lines, spliceargs);
        }
      }
      else if (newText.length == 1) {
        firstLine.replace(from.ch, null, newText[0]);
        lastLine.replace(null, to.ch, "");
        firstLine.append(lastLine);
        lines.splice(from.line + 1, nlines);
      }
      else {
        var spliceargs = [from.line + 1, nlines - 1];
        firstLine.replace(from.ch, null, newText[0]);
        lastLine.replace(null, to.ch, newText[newText.length-1]);
        for (var i = 1, e = newText.length - 1; i < e; ++i)
          spliceargs.push(Line.inheritMarks(newText[i], firstLine));
        lines.splice.apply(lines, spliceargs);
      }


      for (var i = from.line, e = i + newText.length; i < e; ++i) {
        var l = lines[i].text;
        if (l.length > maxLineLength) {
          maxLine = l; maxLineLength = l.length; maxWidth = null;
          recomputeMaxLength = false;
        }
      }
      if (recomputeMaxLength) {
        maxLineLength = 0; maxLine = ""; maxWidth = null;
        for (var i = 0, e = lines.length; i < e; ++i) {
          var l = lines[i].text;
          if (l.length > maxLineLength) {
            maxLineLength = l.length; maxLine = l;
          }
        }
      }

      // Add these lines to the work array, so that they will be
      // highlighted. Adjust work lines if lines were added/removed.
      var newWork = [], lendiff = newText.length - nlines - 1;
      for (var i = 0, l = work.length; i < l; ++i) {
        var task = work[i];
        if (task < from.line) newWork.push(task);
        else if (task > to.line) newWork.push(task + lendiff);
      }
      if (newText.length < 5) {
        highlightLines(from.line, from.line + newText.length);
        newWork.push(from.line + newText.length);
      } else {
        newWork.push(from.line);
      }
      work = newWork;
      startWorker(100);
      // Remember that these lines changed, for updating the display
      changes.push({from: from.line, to: to.line + 1, diff: lendiff});
      textChanged = {from: from, to: to, text: newText};

      // Update the selection
      function updateLine(n) {return n <= Math.min(to.line, to.line + lendiff) ? n : n + lendiff;}
      setSelection(selFrom, selTo, updateLine(sel.from.line), updateLine(sel.to.line));

      // Make sure the scroll-size div has the correct height.
      code.style.height = (lines.length * lineHeight() + 2 * paddingTop()) + "px";
    }

    function replaceRange(code, from, to) {
      from = clipPos(from);
      if (!to) to = from; else to = clipPos(to);
      code = splitLines(code);
      function adjustPos(pos) {
        if (posLess(pos, from)) return pos;
        if (!posLess(to, pos)) return end;
        var line = pos.line + code.length - (to.line - from.line) - 1;
        var ch = pos.ch;
        if (pos.line == to.line)
          ch += code[code.length-1].length - (to.ch - (to.line == from.line ? from.ch : 0));
        return {line: line, ch: ch};
      }
      var end;
      replaceRange1(code, from, to, function(end1) {
        end = end1;
        return {from: adjustPos(sel.from), to: adjustPos(sel.to)};
      });
      return end;
    }
    function replaceSelection(code, collapse) {
      replaceRange1(splitLines(code), sel.from, sel.to, function(end) {
        if (collapse == "end") return {from: end, to: end};
        else if (collapse == "start") return {from: sel.from, to: sel.from};
        else return {from: sel.from, to: end};
      });
    }
    function replaceRange1(code, from, to, computeSel) {
      var endch = code.length == 1 ? code[0].length + from.ch : code[code.length-1].length;
      var newSel = computeSel({line: from.line + code.length - 1, ch: endch});
      updateLines(from, to, code, newSel.from, newSel.to);
    }

    function getRange(from, to) {
      var l1 = from.line, l2 = to.line;
      if (l1 == l2) return lines[l1].text.slice(from.ch, to.ch);
      var code = [lines[l1].text.slice(from.ch)];
      for (var i = l1 + 1; i < l2; ++i) code.push(lines[i].text);
      code.push(lines[l2].text.slice(0, to.ch));
      return code.join("\n");
    }
    function getSelection() {
      return getRange(sel.from, sel.to);
    }

    var pollingFast = false; // Ensures slowPoll doesn't cancel fastPoll
    function slowPoll() {
      if (pollingFast) return;
      poll.set(2000, function() {
        startOperation();
        readInput();
        if (focused) slowPoll();
        endOperation();
      });
    }
    function fastPoll(keyId) {
      var missed = false;
      pollingFast = true;
      function p() {
        startOperation();
        var changed = readInput();
        if (changed && keyId) {
          if (changed == "moved" && movementKeys[keyId] == null) movementKeys[keyId] = true;
          if (changed == "changed") movementKeys[keyId] = false;
        }
        if (!changed && !missed) {missed = true; poll.set(80, p);}
        else {pollingFast = false; slowPoll();}
        endOperation();
      }
      poll.set(20, p);
    }

    // Inspects the textarea, compares its state (content, selection)
    // to the data in the editing variable, and updates the editor
    // content or cursor if something changed.
    function readInput() {
      if (leaveInputAlone || !focused) return;
      var changed = false, text = input.value, sr = selRange(input);
      if (!sr) return false;
      var changed = editing.text != text, rs = reducedSelection;
      var moved = changed || sr.start != editing.start || sr.end != (rs ? editing.start : editing.end);
      if (!moved && !rs) return false;
      if (changed) {
        shiftSelecting = reducedSelection = null;
        if (options.readOnly) {updateInput = true; return "changed";}
      }

      // Compute selection start and end based on start/end offsets in textarea
      function computeOffset(n, startLine) {
        var pos = 0;
        for (;;) {
          var found = text.indexOf("\n", pos);
          if (found == -1 || (text.charAt(found-1) == "\r" ? found - 1 : found) >= n)
            return {line: startLine, ch: n - pos};
          ++startLine;
          pos = found + 1;
        }
      }
      var from = computeOffset(sr.start, editing.from),
          to = computeOffset(sr.end, editing.from);
      // Here we have to take the reducedSelection hack into account,
      // so that you can, for example, press shift-up at the start of
      // your selection and have the right thing happen.
      if (rs) {
        var head = sr.start == rs.anchor ? to : from;
        var tail = shiftSelecting ? sel.to : sr.start == rs.anchor ? from : to;
        if (sel.inverted = posLess(head, tail)) { from = head; to = tail; }
        else { reducedSelection = null; from = tail; to = head; }
      }

      // In some cases (cursor on same line as before), we don't have
      // to update the textarea content at all.
      if (from.line == to.line && from.line == sel.from.line && from.line == sel.to.line && !shiftSelecting)
        updateInput = false;

      // Magic mess to extract precise edited range from the changed
      // string.
      if (changed) {
        var start = 0, end = text.length, len = Math.min(end, editing.text.length);
        var c, line = editing.from, nl = -1;
        while (start < len && (c = text.charAt(start)) == editing.text.charAt(start)) {
          ++start;
          if (c == "\n") {line++; nl = start;}
        }
        var ch = nl > -1 ? start - nl : start, endline = editing.to - 1, edend = editing.text.length;
        for (;;) {
          c = editing.text.charAt(edend);
          if (text.charAt(end) != c) {++end; ++edend; break;}
          if (c == "\n") endline--;
          if (edend <= start || end <= start) break;
          --end; --edend;
        }
        var nl = editing.text.lastIndexOf("\n", edend - 1), endch = nl == -1 ? edend : edend - nl - 1;
        updateLines({line: line, ch: ch}, {line: endline, ch: endch}, splitLines(text.slice(start, end)), from, to);
        if (line != endline || from.line != line) updateInput = true;
      }
      else setSelection(from, to);

      editing.text = text; editing.start = sr.start; editing.end = sr.end;
      return changed ? "changed" : moved ? "moved" : false;
    }

    // Set the textarea content and selection range to match the
    // editor state.
    function prepareInput() {
      var text = [];
      var from = Math.max(0, sel.from.line - 1), to = Math.min(lines.length, sel.to.line + 2);
      for (var i = from; i < to; ++i) text.push(lines[i].text);
      text = input.value = text.join(lineSep);
      var startch = sel.from.ch, endch = sel.to.ch;
      for (var i = from; i < sel.from.line; ++i)
        startch += lineSep.length + lines[i].text.length;
      for (var i = from; i < sel.to.line; ++i)
        endch += lineSep.length + lines[i].text.length;
      editing = {text: text, from: from, to: to, start: startch, end: endch};
      setSelRange(input, startch, reducedSelection ? startch : endch);
    }
    function focusInput() {
      if (options.readOnly != "nocursor") input.focus();
    }

    function scrollEditorIntoView() {
      if (!cursor.getBoundingClientRect) return;
      var rect = cursor.getBoundingClientRect();
      var winH = window.innerHeight || Math.max(document.body.offsetHeight, document.documentElement.offsetHeight);
      if (rect.top < 0 || rect.bottom > winH) cursor.scrollIntoView();
    }
    function scrollCursorIntoView() {
      var cursor = localCoords(sel.inverted ? sel.from : sel.to);
      return scrollIntoView(cursor.x, cursor.y, cursor.x, cursor.yBot);
    }
    function scrollIntoView(x1, y1, x2, y2) {
      var pl = paddingLeft(), pt = paddingTop(), lh = lineHeight();
      y1 += pt; y2 += pt; x1 += pl; x2 += pl;
      var screen = scroller.clientHeight, screentop = scroller.scrollTop, scrolled = false, result = true;
      if (y1 < screentop) {scroller.scrollTop = Math.max(0, y1 - 2*lh); scrolled = true;}
      else if (y2 > screentop + screen) {scroller.scrollTop = y2 + lh - screen; scrolled = true;}

      var screenw = scroller.clientWidth, screenleft = scroller.scrollLeft;
      var gutterw = options.fixedGutter ? gutter.clientWidth : 0;
      if (x1 < screenleft + gutterw) {
        if (x1 < 50) x1 = 0;
        scroller.scrollLeft = Math.max(0, x1 - 10 - gutterw);
        scrolled = true;
      }
      else if (x2 > screenw + screenleft) {
        scroller.scrollLeft = x2 + 10 - screenw;
        scrolled = true;
        if (x2 > code.clientWidth) result = false;
      }
      if (scrolled && options.onScroll) options.onScroll(instance);
      return result;
    }

    function visibleLines() {
      var lh = lineHeight(), top = scroller.scrollTop - paddingTop();
      return {from: Math.min(lines.length, Math.max(0, Math.floor(top / lh))),
              to: Math.min(lines.length, Math.ceil((top + scroller.clientHeight) / lh))};
    }
    // Uses a set of changes plus the current scroll position to
    // determine which DOM updates have to be made, and makes the
    // updates.
    function updateDisplay(changes) {
      if (!scroller.clientWidth) {
        showingFrom = showingTo = 0;
        return;
      }
      // First create a range of theoretically intact lines, and punch
      // holes in that using the change info.
      var intact = changes === true ? [] : [{from: showingFrom, to: showingTo, domStart: 0}];
      for (var i = 0, l = changes.length || 0; i < l; ++i) {
        var change = changes[i], intact2 = [], diff = change.diff || 0;
        for (var j = 0, l2 = intact.length; j < l2; ++j) {
          var range = intact[j];
          if (change.to <= range.from)
            intact2.push({from: range.from + diff, to: range.to + diff, domStart: range.domStart});
          else if (range.to <= change.from)
            intact2.push(range);
          else {
            if (change.from > range.from)
              intact2.push({from: range.from, to: change.from, domStart: range.domStart});
            if (change.to < range.to)
              intact2.push({from: change.to + diff, to: range.to + diff,
                            domStart: range.domStart + (change.to - range.from)});
          }
        }
        intact = intact2;
      }

      // Then, determine which lines we'd want to see, and which
      // updates have to be made to get there.
      var visible = visibleLines();
      var from = Math.min(showingFrom, Math.max(visible.from - 3, 0)),
          to = Math.min(lines.length, Math.max(showingTo, visible.to + 3)),
          updates = [], domPos = 0, domEnd = showingTo - showingFrom, pos = from, changedLines = 0;

      for (var i = 0, l = intact.length; i < l; ++i) {
        var range = intact[i];
        if (range.to <= from) continue;
        if (range.from >= to) break;
        if (range.domStart > domPos || range.from > pos) {
          updates.push({from: pos, to: range.from, domSize: range.domStart - domPos, domStart: domPos});
          changedLines += range.from - pos;
        }
        pos = range.to;
        domPos = range.domStart + (range.to - range.from);
      }
      if (domPos != domEnd || pos != to) {
        changedLines += Math.abs(to - pos);
        updates.push({from: pos, to: to, domSize: domEnd - domPos, domStart: domPos});
        if (to - pos != domEnd - domPos) gutterDirty = true;
      }

      if (!updates.length) return;
      lineDiv.style.display = "none";
      // If more than 30% of the screen needs update, just do a full
      // redraw (which is quicker than patching)
      if (changedLines > (visible.to - visible.from) * .3)
        refreshDisplay(from = Math.max(visible.from - 10, 0), to = Math.min(visible.to + 7, lines.length));
      // Otherwise, only update the stuff that needs updating.
      else
        patchDisplay(updates);
      lineDiv.style.display = "";

      // Position the mover div to align with the lines it's supposed
      // to be showing (which will cover the visible display)
      var different = from != showingFrom || to != showingTo || lastHeight != scroller.clientHeight;
      showingFrom = from; showingTo = to;
      mover.style.top = (from * lineHeight()) + "px";
      if (different) {
        lastHeight = scroller.clientHeight;
        code.style.height = (lines.length * lineHeight() + 2 * paddingTop()) + "px";
      }
      if (different || gutterDirty) updateGutter();

      if (maxWidth == null) maxWidth = stringWidth(maxLine);
      if (maxWidth > scroller.clientWidth) {
        lineSpace.style.width = maxWidth + "px";
        // Needed to prevent odd wrapping/hiding of widgets placed in here.
        code.style.width = "";
        code.style.width = scroller.scrollWidth + "px";
      } else {
        lineSpace.style.width = code.style.width = "";
      }

      // Since this is all rather error prone, it is honoured with the
      // only assertion in the whole file.
      if (lineDiv.childNodes.length != showingTo - showingFrom)
        throw new Error("BAD PATCH! " + JSON.stringify(updates) + " size=" + (showingTo - showingFrom) +
                        " nodes=" + lineDiv.childNodes.length);
      updateCursor();
    }

    function refreshDisplay(from, to) {
      var html = [], start = {line: from, ch: 0}, inSel = posLess(sel.from, start) && !posLess(sel.to, start);
      for (var i = from; i < to; ++i) {
        var ch1 = null, ch2 = null;
        if (inSel) {
          ch1 = 0;
          if (sel.to.line == i) {inSel = false; ch2 = sel.to.ch;}
        }
        else if (sel.from.line == i) {
          if (sel.to.line == i) {ch1 = sel.from.ch; ch2 = sel.to.ch;}
          else {inSel = true; ch1 = sel.from.ch;}
        }
        html.push(lines[i].getHTML(ch1, ch2, true));
      }
      lineDiv.innerHTML = html.join("");
    }
    function patchDisplay(updates) {
      // Slightly different algorithm for IE (badInnerHTML), since
      // there .innerHTML on PRE nodes is dumb, and discards
      // whitespace.
      var sfrom = sel.from.line, sto = sel.to.line, off = 0,
          scratch = badInnerHTML && targetDocument.createElement("div");
      for (var i = 0, e = updates.length; i < e; ++i) {
        var rec = updates[i];
        var extra = (rec.to - rec.from) - rec.domSize;
        var nodeAfter = lineDiv.childNodes[rec.domStart + rec.domSize + off] || null;
        if (badInnerHTML)
          for (var j = Math.max(-extra, rec.domSize); j > 0; --j)
            lineDiv.removeChild(nodeAfter ? nodeAfter.previousSibling : lineDiv.lastChild);
        else if (extra) {
          for (var j = Math.max(0, extra); j > 0; --j)
            lineDiv.insertBefore(targetDocument.createElement("pre"), nodeAfter);
          for (var j = Math.max(0, -extra); j > 0; --j)
            lineDiv.removeChild(nodeAfter ? nodeAfter.previousSibling : lineDiv.lastChild);
        }
        var node = lineDiv.childNodes[rec.domStart + off], inSel = sfrom < rec.from && sto >= rec.from;
        for (var j = rec.from; j < rec.to; ++j) {
          var ch1 = null, ch2 = null;
          if (inSel) {
            ch1 = 0;
            if (sto == j) {inSel = false; ch2 = sel.to.ch;}
          }
          else if (sfrom == j) {
            if (sto == j) {ch1 = sel.from.ch; ch2 = sel.to.ch;}
            else {inSel = true; ch1 = sel.from.ch;}
          }
          if (badInnerHTML) {
            scratch.innerHTML = lines[j].getHTML(ch1, ch2, true);
            lineDiv.insertBefore(scratch.firstChild, nodeAfter);
          }
          else {
            node.innerHTML = lines[j].getHTML(ch1, ch2, false);
            node.className = lines[j].className || "";
            node = node.nextSibling;
          }
        }
        off += extra;
      }
    }

    function updateGutter() {
      if (!options.gutter && !options.lineNumbers) return;
      var hText = mover.offsetHeight, hEditor = scroller.clientHeight;
      gutter.style.height = (hText - hEditor < 2 ? hEditor : hText) + "px";
      var html = [];
      for (var i = showingFrom; i < Math.max(showingTo, showingFrom + 1); ++i) {
        var marker = lines[i].gutterMarker;
        var text = options.lineNumbers ? i + options.firstLineNumber : null;
        if (marker && marker.text)
          text = marker.text.replace("%N%", text != null ? text : "");
        else if (text == null)
          text = "\u00a0";
        html.push((marker && marker.style ? '<pre class="' + marker.style + '">' : "<pre>"), text, "</pre>");
      }
      gutter.style.display = "none";
      gutterText.innerHTML = html.join("");
      var minwidth = String(lines.length).length, firstNode = gutterText.firstChild, val = eltText(firstNode), pad = "";
      while (val.length + pad.length < minwidth) pad += "\u00a0";
      if (pad) firstNode.insertBefore(targetDocument.createTextNode(pad), firstNode.firstChild);
      gutter.style.display = "";
      lineSpace.style.marginLeft = gutter.offsetWidth + "px";
      gutterDirty = false;
    }
    function updateCursor() {
      var head = sel.inverted ? sel.from : sel.to, lh = lineHeight();
      var x = charX(head.line, head.ch);
      var top = head.line * lh - scroller.scrollTop;
      inputDiv.style.top = Math.max(Math.min(top, scroller.offsetHeight), 0) + "px";
      inputDiv.style.left = (x - scroller.scrollLeft) + "px";
      if (posEq(sel.from, sel.to)) {
        cursor.style.top = (head.line - showingFrom) * lh + "px";
        cursor.style.left = x + "px";
        cursor.style.display = "";
      }
      else cursor.style.display = "none";
    }

    function setSelectionUser(from, to) {
      var sh = shiftSelecting && clipPos(shiftSelecting);
      if (sh) {
        if (posLess(sh, from)) from = sh;
        else if (posLess(to, sh)) to = sh;
      }
      setSelection(from, to);
    }
    // Update the selection. Last two args are only used by
    // updateLines, since they have to be expressed in the line
    // numbers before the update.
    function setSelection(from, to, oldFrom, oldTo) {
      if (posEq(sel.from, from) && posEq(sel.to, to)) return;
      if (posLess(to, from)) {var tmp = to; to = from; from = tmp;}

      if (posEq(from, to)) sel.inverted = false;
      else if (posEq(from, sel.to)) sel.inverted = false;
      else if (posEq(to, sel.from)) sel.inverted = true;

      // Some ugly logic used to only mark the lines that actually did
      // see a change in selection as changed, rather than the whole
      // selected range.
      if (oldFrom == null) {oldFrom = sel.from.line; oldTo = sel.to.line;}
      if (posEq(from, to)) {
        if (!posEq(sel.from, sel.to))
          changes.push({from: oldFrom, to: oldTo + 1});
      }
      else if (posEq(sel.from, sel.to)) {
        changes.push({from: from.line, to: to.line + 1});
      }
      else {
        if (!posEq(from, sel.from)) {
          if (from.line < oldFrom)
            changes.push({from: from.line, to: Math.min(to.line, oldFrom) + 1});
          else
            changes.push({from: oldFrom, to: Math.min(oldTo, from.line) + 1});
        }
        if (!posEq(to, sel.to)) {
          if (to.line < oldTo)
            changes.push({from: Math.max(oldFrom, from.line), to: oldTo + 1});
          else
            changes.push({from: Math.max(from.line, oldTo), to: to.line + 1});
        }
      }
      sel.from = from; sel.to = to;
      selectionChanged = true;
    }
    function setCursor(line, ch, user) {
      var pos = clipPos({line: line, ch: ch || 0});
      (user ? setSelectionUser : setSelection)(pos, pos);
    }

    function clipLine(n) {return Math.max(0, Math.min(n, lines.length-1));}
    function clipPos(pos) {
      if (pos.line < 0) return {line: 0, ch: 0};
      if (pos.line >= lines.length) return {line: lines.length-1, ch: lines[lines.length-1].text.length};
      var ch = pos.ch, linelen = lines[pos.line].text.length;
      if (ch == null || ch > linelen) return {line: pos.line, ch: linelen};
      else if (ch < 0) return {line: pos.line, ch: 0};
      else return pos;
    }

    function scrollPage(down) {
      var linesPerPage = Math.floor(scroller.clientHeight / lineHeight()), head = sel.inverted ? sel.from : sel.to;
      setCursor(head.line + (Math.max(linesPerPage - 1, 1) * (down ? 1 : -1)), head.ch, true);
    }
    function scrollEnd(top) {
      var pos = top ? {line: 0, ch: 0} : {line: lines.length - 1, ch: lines[lines.length-1].text.length};
      setSelectionUser(pos, pos);
    }
    function selectAll() {
      var endLine = lines.length - 1;
      setSelection({line: 0, ch: 0}, {line: endLine, ch: lines[endLine].text.length});
    }
    function selectWordAt(pos) {
      var line = lines[pos.line].text;
      var start = pos.ch, end = pos.ch;
      while (start > 0 && /\w/.test(line.charAt(start - 1))) --start;
      while (end < line.length && /\w/.test(line.charAt(end))) ++end;
      setSelectionUser({line: pos.line, ch: start}, {line: pos.line, ch: end});
    }
    function selectLine(line) {
      setSelectionUser({line: line, ch: 0}, {line: line, ch: lines[line].text.length});
    }
    function handleEnter() {
      replaceSelection("\n", "end");
      if (options.enterMode != "flat")
        indentLine(sel.from.line, options.enterMode == "keep" ? "prev" : "smart");
    }
    function handleTab(shift) {
      function indentSelected(mode) {
        if (posEq(sel.from, sel.to)) return indentLine(sel.from.line, mode);
        var e = sel.to.line - (sel.to.ch ? 0 : 1);
        for (var i = sel.from.line; i <= e; ++i) indentLine(i, mode);
      }
      shiftSelecting = null;
      switch (options.tabMode) {
      case "default":
        return false;
      case "indent":
        indentSelected("smart");
        break;
      case "classic":
        if (posEq(sel.from, sel.to)) {
          if (shift) indentLine(sel.from.line, "smart");
          else replaceSelection("\t", "end");
          break;
        }
      case "shift":
        indentSelected(shift ? "subtract" : "add");
        break;
      }
      return true;
    }
    function smartHome() {
      var firstNonWS = Math.max(0, lines[sel.from.line].text.search(/\S/));
      setCursor(sel.from.line, sel.from.ch <= firstNonWS && sel.from.ch ? 0 : firstNonWS, true);
    }

    function indentLine(n, how) {
      if (how == "smart") {
        if (!mode.indent) how = "prev";
        else var state = getStateBefore(n);
      }

      var line = lines[n], curSpace = line.indentation(), curSpaceString = line.text.match(/^\s*/)[0], indentation;
      if (how == "prev") {
        if (n) indentation = lines[n-1].indentation();
        else indentation = 0;
      }
      else if (how == "smart") indentation = mode.indent(state, line.text.slice(curSpaceString.length));
      else if (how == "add") indentation = curSpace + options.indentUnit;
      else if (how == "subtract") indentation = curSpace - options.indentUnit;
      indentation = Math.max(0, indentation);
      var diff = indentation - curSpace;

      if (!diff) {
        if (sel.from.line != n && sel.to.line != n) return;
        var indentString = curSpaceString;
      }
      else {
        var indentString = "", pos = 0;
        if (options.indentWithTabs)
          for (var i = Math.floor(indentation / tabSize); i; --i) {pos += tabSize; indentString += "\t";}
        while (pos < indentation) {++pos; indentString += " ";}
      }

      replaceRange(indentString, {line: n, ch: 0}, {line: n, ch: curSpaceString.length});
    }

    function loadMode() {
      mode = CodeMirror.getMode(options, options.mode);
      for (var i = 0, l = lines.length; i < l; ++i)
        lines[i].stateAfter = null;
      work = [0];
      startWorker();
    }
    function gutterChanged() {
      var visible = options.gutter || options.lineNumbers;
      gutter.style.display = visible ? "" : "none";
      if (visible) gutterDirty = true;
      else lineDiv.parentNode.style.marginLeft = 0;
    }

    function markText(from, to, className) {
      from = clipPos(from); to = clipPos(to);
      var set = [];
      function add(line, from, to, className) {
        mark = lines[line].addMark(from, to, className, set);
      }
      if (from.line == to.line) add(from.line, from.ch, to.ch, className);
      else {
        add(from.line, from.ch, null, className);
        for (var i = from.line + 1, e = to.line; i < e; ++i)
          add(i, 0, null, className);
        add(to.line, 0, to.ch, className);
      }
      changes.push({from: from.line, to: to.line + 1});
      return new TextMarker(set);
    }

    function TextMarker(set) { this.set = set; }
    TextMarker.prototype.clear = operation(function() {
      for (var i = 0, e = this.set.length; i < e; ++i) {
        var mk = this.set[i].marked;
        for (var j = 0; j < mk.length; ++j) {
          if (mk[j].set == this.set) mk.splice(j--, 1);
        }
      }
      // We don't know the exact lines that changed. Refreshing is
      // cheaper than finding them.
      changes.push({from: 0, to: lines.length});
    });
    TextMarker.prototype.find = function() {
      var from, to;
      for (var i = 0, e = this.set.length; i < e; ++i) {
        var line = this.set[i], mk = line.marked;
        for (var j = 0; j < mk.length; ++j) {
          var mark = mk[j];
          if (mark.set == this.set) {
            if (mark.from != null || mark.to != null) {
              var found = indexOf(lines, line);
              if (found > -1) {
                if (mark.from != null) from = {line: found, ch: mark.from};
                if (mark.to != null) to = {line: found, ch: mark.to};
              }
            }
          }
        }
      }
      return {from: from, to: to};
    };

    function addGutterMarker(line, text, className) {
      if (typeof line == "number") line = lines[clipLine(line)];
      line.gutterMarker = {text: text, style: className};
      gutterDirty = true;
      return line;
    }
    function removeGutterMarker(line) {
      if (typeof line == "number") line = lines[clipLine(line)];
      line.gutterMarker = null;
      gutterDirty = true;
    }
    function setLineClass(line, className) {
      if (typeof line == "number") {
        var no = line;
        line = lines[clipLine(line)];
      }
      else {
        var no = indexOf(lines, line);
        if (no == -1) return null;
      }
      if (line.className != className) {
        line.className = className;
        changes.push({from: no, to: no + 1});
      }
      return line;
    }

    function lineInfo(line) {
      if (typeof line == "number") {
        var n = line;
        line = lines[line];
        if (!line) return null;
      }
      else {
        var n = indexOf(lines, line);
        if (n == -1) return null;
      }
      var marker = line.gutterMarker;
      return {line: n, text: line.text, markerText: marker && marker.text, markerClass: marker && marker.style};
    }

    function stringWidth(str) {
      measure.innerHTML = "<pre><span>x</span></pre>";
      measure.firstChild.firstChild.firstChild.nodeValue = str;
      return measure.firstChild.firstChild.offsetWidth || 10;
    }
    // These are used to go from pixel positions to character
    // positions, taking varying character widths into account.
    function charX(line, pos) {
      if (pos == 0) return 0;
      measure.innerHTML = "<pre><span>" + lines[line].getHTML(null, null, false, pos) + "</span></pre>";
      return measure.firstChild.firstChild.offsetWidth;
    }
    function charFromX(line, x) {
      if (x <= 0) return 0;
      var lineObj = lines[line], text = lineObj.text;
      function getX(len) {
        measure.innerHTML = "<pre><span>" + lineObj.getHTML(null, null, false, len) + "</span></pre>";
        return measure.firstChild.firstChild.offsetWidth;
      }
      var from = 0, fromX = 0, to = text.length, toX;
      // Guess a suitable upper bound for our search.
      var estimated = Math.min(to, Math.ceil(x / stringWidth("x")));
      for (;;) {
        var estX = getX(estimated);
        if (estX <= x && estimated < to) estimated = Math.min(to, Math.ceil(estimated * 1.2));
        else {toX = estX; to = estimated; break;}
      }
      if (x > toX) return to;
      // Try to guess a suitable lower bound as well.
      estimated = Math.floor(to * 0.8); estX = getX(estimated);
      if (estX < x) {from = estimated; fromX = estX;}
      // Do a binary search between these bounds.
      for (;;) {
        if (to - from <= 1) return (toX - x > x - fromX) ? from : to;
        var middle = Math.ceil((from + to) / 2), middleX = getX(middle);
        if (middleX > x) {to = middle; toX = middleX;}
        else {from = middle; fromX = middleX;}
      }
    }

    function localCoords(pos, inLineWrap) {
      var lh = lineHeight(), line = pos.line - (inLineWrap ? showingFrom : 0);
      return {x: charX(pos.line, pos.ch), y: line * lh, yBot: (line + 1) * lh};
    }
    function pageCoords(pos) {
      var local = localCoords(pos, true), off = eltOffset(lineSpace);
      return {x: off.left + local.x, y: off.top + local.y, yBot: off.top + local.yBot};
    }

    function lineHeight() {
      var nlines = lineDiv.childNodes.length;
      if (nlines) return (lineDiv.offsetHeight / nlines) || 1;
      measure.innerHTML = "<pre>x</pre>";
      return measure.firstChild.offsetHeight || 1;
    }
    function paddingTop() {return lineSpace.offsetTop;}
    function paddingLeft() {return lineSpace.offsetLeft;}

    function posFromMouse(e, liberal) {
      var offW = eltOffset(scroller, true), x, y;
      // Fails unpredictably on IE[67] when mouse is dragged around quickly.
      try { x = e.clientX; y = e.clientY; } catch (e) { return null; }
      // This is a mess of a heuristic to try and determine whether a
      // scroll-bar was clicked or not, and to return null if one was
      // (and !liberal).
      if (!liberal && (x - offW.left > scroller.clientWidth || y - offW.top > scroller.clientHeight))
        return null;
      var offL = eltOffset(lineSpace, true);
      var line = showingFrom + Math.floor((y - offL.top) / lineHeight());
      return clipPos({line: line, ch: charFromX(clipLine(line), x - offL.left)});
    }
    function onContextMenu(e) {
      var pos = posFromMouse(e);
      if (!pos || window.opera) return; // Opera is difficult.
      if (posEq(sel.from, sel.to) || posLess(pos, sel.from) || !posLess(pos, sel.to))
        operation(setCursor)(pos.line, pos.ch);

      var oldCSS = input.style.cssText;
      inputDiv.style.position = "absolute";
      input.style.cssText = "position: fixed; width: 30px; height: 30px; top: " + (e.clientY - 5) +
        "px; left: " + (e.clientX - 5) + "px; z-index: 1000; background: white; " +
        "border-width: 0; outline: none; overflow: hidden; opacity: .05; filter: alpha(opacity=5);";
      leaveInputAlone = true;
      var val = input.value = getSelection();
      focusInput();
      setSelRange(input, 0, input.value.length);
      function rehide() {
        var newVal = splitLines(input.value).join("\n");
        if (newVal != val) operation(replaceSelection)(newVal, "end");
        inputDiv.style.position = "relative";
        input.style.cssText = oldCSS;
        leaveInputAlone = false;
        prepareInput();
        slowPoll();
      }

      if (gecko) {
        e_stop(e);
        var mouseup = connect(window, "mouseup", function() {
          mouseup();
          setTimeout(rehide, 20);
        }, true);
      }
      else {
        setTimeout(rehide, 50);
      }
    }

    // Cursor-blinking
    function restartBlink() {
      clearInterval(blinker);
      var on = true;
      cursor.style.visibility = "";
      blinker = setInterval(function() {
        cursor.style.visibility = (on = !on) ? "" : "hidden";
      }, 650);
    }

    var matching = {"(": ")>", ")": "(<", "[": "]>", "]": "[<", "{": "}>", "}": "{<"};
    function matchBrackets(autoclear) {
      var head = sel.inverted ? sel.from : sel.to, line = lines[head.line], pos = head.ch - 1;
      var match = (pos >= 0 && matching[line.text.charAt(pos)]) || matching[line.text.charAt(++pos)];
      if (!match) return;
      var ch = match.charAt(0), forward = match.charAt(1) == ">", d = forward ? 1 : -1, st = line.styles;
      for (var off = pos + 1, i = 0, e = st.length; i < e; i+=2)
        if ((off -= st[i].length) <= 0) {var style = st[i+1]; break;}

      var stack = [line.text.charAt(pos)], re = /[(){}[\]]/;
      function scan(line, from, to) {
        if (!line.text) return;
        var st = line.styles, pos = forward ? 0 : line.text.length - 1, cur;
        for (var i = forward ? 0 : st.length - 2, e = forward ? st.length : -2; i != e; i += 2*d) {
          var text = st[i];
          if (st[i+1] != null && st[i+1] != style) {pos += d * text.length; continue;}
          for (var j = forward ? 0 : text.length - 1, te = forward ? text.length : -1; j != te; j += d, pos+=d) {
            if (pos >= from && pos < to && re.test(cur = text.charAt(j))) {
              var match = matching[cur];
              if (match.charAt(1) == ">" == forward) stack.push(cur);
              else if (stack.pop() != match.charAt(0)) return {pos: pos, match: false};
              else if (!stack.length) return {pos: pos, match: true};
            }
          }
        }
      }
      for (var i = head.line, e = forward ? Math.min(i + 100, lines.length) : Math.max(-1, i - 100); i != e; i+=d) {
        var line = lines[i], first = i == head.line;
        var found = scan(line, first && forward ? pos + 1 : 0, first && !forward ? pos : line.text.length);
        if (found) break;
      }
      if (!found) found = {pos: null, match: false};
      var style = found.match ? "CodeMirror-matchingbracket" : "CodeMirror-nonmatchingbracket";
      var one = markText({line: head.line, ch: pos}, {line: head.line, ch: pos+1}, style),
          two = found.pos != null && markText({line: i, ch: found.pos}, {line: i, ch: found.pos + 1}, style);
      var clear = operation(function(){one.clear(); two && two.clear();});
      if (autoclear) setTimeout(clear, 800);
      else bracketHighlighted = clear;
    }

    // Finds the line to start with when starting a parse. Tries to
    // find a line with a stateAfter, so that it can start with a
    // valid state. If that fails, it returns the line with the
    // smallest indentation, which tends to need the least context to
    // parse correctly.
    function findStartLine(n) {
      var minindent, minline;
      for (var search = n, lim = n - 40; search > lim; --search) {
        if (search == 0) return 0;
        var line = lines[search-1];
        if (line.stateAfter) return search;
        var indented = line.indentation();
        if (minline == null || minindent > indented) {
          minline = search - 1;
          minindent = indented;
        }
      }
      return minline;
    }
    function getStateBefore(n) {
      var start = findStartLine(n), state = start && lines[start-1].stateAfter;
      if (!state) state = startState(mode);
      else state = copyState(mode, state);
      for (var i = start; i < n; ++i) {
        var line = lines[i];
        line.highlight(mode, state);
        line.stateAfter = copyState(mode, state);
      }
      changes.push({from: start, to: n});
      if (n < lines.length && !lines[n].stateAfter) work.push(n);
      return state;
    }
    function highlightLines(start, end) {
      var state = getStateBefore(start);
      for (var i = start; i < end; ++i) {
        var line = lines[i];
        line.highlight(mode, state);
        line.stateAfter = copyState(mode, state);
      }
    }
    function highlightWorker() {
      var end = +new Date + options.workTime;
      var foundWork = work.length;
      while (work.length) {
        if (!lines[showingFrom].stateAfter) var task = showingFrom;
        else var task = work.pop();
        if (task >= lines.length) continue;
        var start = findStartLine(task), state = start && lines[start-1].stateAfter;
        if (state) state = copyState(mode, state);
        else state = startState(mode);

        var unchanged = 0, compare = mode.compareStates, realChange = false;
        for (var i = start, l = lines.length; i < l; ++i) {
          var line = lines[i], hadState = line.stateAfter;
          if (+new Date > end) {
            work.push(i);
            startWorker(options.workDelay);
            if (realChange) changes.push({from: task, to: i + 1});
            return;
          }
          var changed = line.highlight(mode, state);
          if (changed) realChange = true;
          line.stateAfter = copyState(mode, state);
          if (compare) {
            if (hadState && compare(hadState, state)) break;
          } else {
            if (changed !== false || !hadState) unchanged = 0;
            else if (++unchanged > 3) break;
          }
        }
        if (realChange) changes.push({from: task, to: i + 1});
      }
      if (foundWork && options.onHighlightComplete)
        options.onHighlightComplete(instance);
    }
    function startWorker(time) {
      if (!work.length) return;
      highlight.set(time, operation(highlightWorker));
    }

    // Operations are used to wrap changes in such a way that each
    // change won't have to update the cursor and display (which would
    // be awkward, slow, and error-prone), but instead updates are
    // batched and then all combined and executed at once.
    function startOperation() {
      updateInput = null; changes = []; textChanged = selectionChanged = false;
    }
    function endOperation() {
      var reScroll = false;
      if (selectionChanged) reScroll = !scrollCursorIntoView();
      if (changes.length) updateDisplay(changes);
      else {
        if (selectionChanged) updateCursor();
        if (gutterDirty) updateGutter();
      }
      if (reScroll) scrollCursorIntoView();
      if (selectionChanged) {scrollEditorIntoView(); restartBlink();}

      // updateInput can be set to a boolean value to force/prevent an
      // update.
      if (focused && !leaveInputAlone &&
          (updateInput === true || (updateInput !== false && selectionChanged)))
        prepareInput();

      if (selectionChanged && options.matchBrackets)
        setTimeout(operation(function() {
          if (bracketHighlighted) {bracketHighlighted(); bracketHighlighted = null;}
          matchBrackets(false);
        }), 20);
      var tc = textChanged; // textChanged can be reset by cursoractivity callback
      if (selectionChanged && options.onCursorActivity)
        options.onCursorActivity(instance);
      if (tc && options.onChange && instance)
        options.onChange(instance, tc);
    }
    var nestedOperation = 0;
    function operation(f) {
      return function() {
        if (!nestedOperation++) startOperation();
        try {var result = f.apply(this, arguments);}
        finally {if (!--nestedOperation) endOperation();}
        return result;
      };
    }

    function SearchCursor(query, pos, caseFold) {
      this.atOccurrence = false;
      if (caseFold == null) caseFold = typeof query == "string" && query == query.toLowerCase();

      if (pos && typeof pos == "object") pos = clipPos(pos);
      else pos = {line: 0, ch: 0};
      this.pos = {from: pos, to: pos};

      // The matches method is filled in based on the type of query.
      // It takes a position and a direction, and returns an object
      // describing the next occurrence of the query, or null if no
      // more matches were found.
      if (typeof query != "string") // Regexp match
        this.matches = function(reverse, pos) {
          if (reverse) {
            var line = lines[pos.line].text.slice(0, pos.ch), match = line.match(query), start = 0;
            while (match) {
              var ind = line.indexOf(match[0]);
              start += ind;
              line = line.slice(ind + 1);
              var newmatch = line.match(query);
              if (newmatch) match = newmatch;
              else break;
              start++;
            }
          }
          else {
            var line = lines[pos.line].text.slice(pos.ch), match = line.match(query),
                start = match && pos.ch + line.indexOf(match[0]);
          }
          if (match)
            return {from: {line: pos.line, ch: start},
                    to: {line: pos.line, ch: start + match[0].length},
                    match: match};
        };
      else { // String query
        if (caseFold) query = query.toLowerCase();
        var fold = caseFold ? function(str){return str.toLowerCase();} : function(str){return str;};
        var target = query.split("\n");
        // Different methods for single-line and multi-line queries
        if (target.length == 1)
          this.matches = function(reverse, pos) {
            var line = fold(lines[pos.line].text), len = query.length, match;
            if (reverse ? (pos.ch >= len && (match = line.lastIndexOf(query, pos.ch - len)) != -1)
                        : (match = line.indexOf(query, pos.ch)) != -1)
              return {from: {line: pos.line, ch: match},
                      to: {line: pos.line, ch: match + len}};
          };
        else
          this.matches = function(reverse, pos) {
            var ln = pos.line, idx = (reverse ? target.length - 1 : 0), match = target[idx], line = fold(lines[ln].text);
            var offsetA = (reverse ? line.indexOf(match) + match.length : line.lastIndexOf(match));
            if (reverse ? offsetA >= pos.ch || offsetA != match.length
                        : offsetA <= pos.ch || offsetA != line.length - match.length)
              return;
            for (;;) {
              if (reverse ? !ln : ln == lines.length - 1) return;
              line = fold(lines[ln += reverse ? -1 : 1].text);
              match = target[reverse ? --idx : ++idx];
              if (idx > 0 && idx < target.length - 1) {
                if (line != match) return;
                else continue;
              }
              var offsetB = (reverse ? line.lastIndexOf(match) : line.indexOf(match) + match.length);
              if (reverse ? offsetB != line.length - match.length : offsetB != match.length)
                return;
              var start = {line: pos.line, ch: offsetA}, end = {line: ln, ch: offsetB};
              return {from: reverse ? end : start, to: reverse ? start : end};
            }
          };
      }
    }

    SearchCursor.prototype = {
      findNext: function() {return this.find(false);},
      findPrevious: function() {return this.find(true);},

      find: function(reverse) {
        var self = this, pos = clipPos(reverse ? this.pos.from : this.pos.to);
        function savePosAndFail(line) {
          var pos = {line: line, ch: 0};
          self.pos = {from: pos, to: pos};
          self.atOccurrence = false;
          return false;
        }

        for (;;) {
          if (this.pos = this.matches(reverse, pos)) {
            this.atOccurrence = true;
            return this.pos.match || true;
          }
          if (reverse) {
            if (!pos.line) return savePosAndFail(0);
            pos = {line: pos.line-1, ch: lines[pos.line-1].text.length};
          }
          else {
            if (pos.line == lines.length - 1) return savePosAndFail(lines.length);
            pos = {line: pos.line+1, ch: 0};
          }
        }
      },

      from: function() {if (this.atOccurrence) return copyPos(this.pos.from);},
      to: function() {if (this.atOccurrence) return copyPos(this.pos.to);},

      replace: function(newText) {
        var self = this;
        if (this.atOccurrence)
          operation(function() {
            self.pos.to = replaceRange(newText, self.pos.from, self.pos.to);
          })();
      }
    };

    for (var ext in extensions)
      if (extensions.propertyIsEnumerable(ext) &&
          !instance.propertyIsEnumerable(ext))
        instance[ext] = extensions[ext];
    return instance;
  } // (end of function CodeMirror)

  // The default configuration options.
  CodeMirror.defaults = {
    value: "",
    mode: null,
    theme: "default",
    indentUnit: 2,
    indentWithTabs: false,
    tabMode: "classic",
    enterMode: "indent",
    electricChars: true,
    onKeyEvent: null,
    lineNumbers: false,
    gutter: false,
    fixedGutter: false,
    firstLineNumber: 1,
    readOnly: false,
    smartHome: true,
    onChange: null,
    onCursorActivity: null,
    onGutterClick: null,
    onHighlightComplete: null,
    onFocus: null, onBlur: null, onScroll: null,
    matchBrackets: false,
    workTime: 100,
    workDelay: 200,
    undoDepth: 40,
    tabindex: null,
    document: window.document
  };

  // Known modes, by name and by MIME
  var modes = {}, mimeModes = {};
  CodeMirror.defineMode = function(name, mode) {
    if (!CodeMirror.defaults.mode && name != "null") CodeMirror.defaults.mode = name;
    modes[name] = mode;
  };
  CodeMirror.defineMIME = function(mime, spec) {
    mimeModes[mime] = spec;
  };
  CodeMirror.getMode = function(options, spec) {
    if (typeof spec == "string" && mimeModes.hasOwnProperty(spec))
      spec = mimeModes[spec];
    if (typeof spec == "string")
      var mname = spec, config = {};
    else if (spec != null)
      var mname = spec.name, config = spec;
    var mfactory = modes[mname];
    if (!mfactory) {
      if (window.console) console.warn("No mode " + mname + " found, falling back to plain text.");
      return CodeMirror.getMode(options, "text/plain");
    }
    return mfactory(options, config || {});
  };
  CodeMirror.listModes = function() {
    var list = [];
    for (var m in modes)
      if (modes.propertyIsEnumerable(m)) list.push(m);
    return list;
  };
  CodeMirror.listMIMEs = function() {
    var list = [];
    for (var m in mimeModes)
      if (mimeModes.propertyIsEnumerable(m)) list.push({mime: m, mode: mimeModes[m]});
    return list;
  };

  var extensions = {};
  CodeMirror.defineExtension = function(name, func) {
    extensions[name] = func;
  };

  CodeMirror.fromTextArea = function(textarea, options) {
    if (!options) options = {};
    options.value = textarea.value;
    if (!options.tabindex && textarea.tabindex)
      options.tabindex = textarea.tabindex;

    function save() {textarea.value = instance.getValue();}
    if (textarea.form) {
      // Deplorable hack to make the submit method do the right thing.
      var rmSubmit = connect(textarea.form, "submit", save, true);
      if (typeof textarea.form.submit == "function") {
        var realSubmit = textarea.form.submit;
        function wrappedSubmit() {
          save();
          textarea.form.submit = realSubmit;
          textarea.form.submit();
          textarea.form.submit = wrappedSubmit;
        }
        textarea.form.submit = wrappedSubmit;
      }
    }

    textarea.style.display = "none";
    var instance = CodeMirror(function(node) {
      textarea.parentNode.insertBefore(node, textarea.nextSibling);
    }, options);
    instance.save = save;
    instance.toTextArea = function() {
      save();
      textarea.parentNode.removeChild(instance.getWrapperElement());
      textarea.style.display = "";
      if (textarea.form) {
        rmSubmit();
        if (typeof textarea.form.submit == "function")
          textarea.form.submit = realSubmit;
      }
    };
    return instance;
  };

  // Utility functions for working with state. Exported because modes
  // sometimes need to do this.
  function copyState(mode, state) {
    if (state === true) return state;
    if (mode.copyState) return mode.copyState(state);
    var nstate = {};
    for (var n in state) {
      var val = state[n];
      if (val instanceof Array) val = val.concat([]);
      nstate[n] = val;
    }
    return nstate;
  }
  CodeMirror.startState = startState;
  function startState(mode, a1, a2) {
    return mode.startState ? mode.startState(a1, a2) : true;
  }
  CodeMirror.copyState = copyState;

  // The character stream used by a mode's parser.
  function StringStream(string) {
    this.pos = this.start = 0;
    this.string = string;
  }
  StringStream.prototype = {
    eol: function() {return this.pos >= this.string.length;},
    sol: function() {return this.pos == 0;},
    peek: function() {return this.string.charAt(this.pos);},
    next: function() {
      if (this.pos < this.string.length)
        return this.string.charAt(this.pos++);
    },
    eat: function(match) {
      var ch = this.string.charAt(this.pos);
      if (typeof match == "string") var ok = ch == match;
      else var ok = ch && (match.test ? match.test(ch) : match(ch));
      if (ok) {++this.pos; return ch;}
    },
    eatWhile: function(match) {
      var start = this.pos;
      while (this.eat(match)){}
      return this.pos > start;
    },
    eatSpace: function() {
      var start = this.pos;
      while (/[\s\u00a0]/.test(this.string.charAt(this.pos))) ++this.pos;
      return this.pos > start;
    },
    skipToEnd: function() {this.pos = this.string.length;},
    skipTo: function(ch) {
      var found = this.string.indexOf(ch, this.pos);
      if (found > -1) {this.pos = found; return true;}
    },
    backUp: function(n) {this.pos -= n;},
    column: function() {return countColumn(this.string, this.start);},
    indentation: function() {return countColumn(this.string);},
    match: function(pattern, consume, caseInsensitive) {
      if (typeof pattern == "string") {
        function cased(str) {return caseInsensitive ? str.toLowerCase() : str;}
        if (cased(this.string).indexOf(cased(pattern), this.pos) == this.pos) {
          if (consume !== false) this.pos += pattern.length;
          return true;
        }
      }
      else {
        var match = this.string.slice(this.pos).match(pattern);
        if (match && consume !== false) this.pos += match[0].length;
        return match;
      }
    },
    current: function(){return this.string.slice(this.start, this.pos);}
  };
  CodeMirror.StringStream = StringStream;

  // Line objects. These hold state related to a line, including
  // highlighting info (the styles array).
  function Line(text, styles) {
    this.styles = styles || [text, null];
    this.stateAfter = null;
    this.text = text;
    this.marked = this.gutterMarker = this.className = null;
  }
  Line.inheritMarks = function(text, orig) {
    var ln = new Line(text), mk = orig.marked;
    if (mk) {
      for (var i = 0; i < mk.length; ++i) {
        if (mk[i].to == null) {
          var newmk = ln.marked || (ln.marked = []), mark = mk[i];
          newmk.push({from: null, to: null, style: mark.style, set: mark.set});
          mark.set.push(ln);
        }
      }
    }
    return ln;
  }
  Line.prototype = {
    // Replace a piece of a line, keeping the styles around it intact.
    replace: function(from, to_, text) {
      var st = [], mk = this.marked, to = to_ == null ? this.text.length : to_;
      copyStyles(0, from, this.styles, st);
      if (text) st.push(text, null);
      copyStyles(to, this.text.length, this.styles, st);
      this.styles = st;
      this.text = this.text.slice(0, from) + text + this.text.slice(to);
      this.stateAfter = null;
      if (mk) {
        var diff = text.length - (to - from), end = this.text.length;
        var changeStart = Math.min(from, from + diff);
        for (var i = 0; i < mk.length; ++i) {
          var mark = mk[i], del = false;
          if (mark.from != null && mark.from >= end) del = true;
          else {
            if (mark.from != null && mark.from >= from) {
              mark.from += diff;
              if (mark.from <= 0) mark.from = from == null ? null : 0;
            }
            else if (to_ == null) mark.to = null;
            if (mark.to != null && mark.to > from) {
              mark.to += diff;
              if (mark.to < 0) del = true;
            }
          }
          if (del || (mark.from != null && mark.to != null && mark.from >= mark.to)) mk.splice(i--, 1);
        }
      }
    },
    // Split a part off a line, keeping styles and markers intact.
    split: function(pos, textBefore) {
      var st = [textBefore, null], mk = this.marked;
      copyStyles(pos, this.text.length, this.styles, st);
      var taken = new Line(textBefore + this.text.slice(pos), st);
      if (mk) {
        for (var i = 0; i < mk.length; ++i) {
          var mark = mk[i];
          if (mark.to > pos || mark.to == null) {
            if (!taken.marked) taken.marked = [];
            taken.marked.push({
              from: mark.from < pos || mark.from == null ? null : mark.from - pos + textBefore.length,
              to: mark.to == null ? null : mark.to - pos + textBefore.length,
              style: mark.style, set: mark.set
            });
            mark.set.push(taken);
          }
        }
      }
      return taken;
    },
    append: function(line) {
      if (!line.text.length) return;
      var mylen = this.text.length, mk = line.marked;
      this.text += line.text;
      copyStyles(0, line.text.length, line.styles, this.styles);
      if (mk && mk.length) {
        var mymk = this.marked || (this.marked = []);
        for (var i = 0; i < mymk.length; ++i)
          if (mymk[i].to == null) mymk[i].to = mylen;
        outer: for (var i = 0; i < mk.length; ++i) {
          var mark = mk[i];
          if (!mark.from) {
            for (var j = 0; j < mymk.length; ++j) {
              var mymark = mymk[j];
              if (mymark.to == mylen && mymark.set == mark.set) {
                mymark.to = mark.to == null ? null : mark.to + mylen;
                continue outer;
              }
            }
          }
          mymk.push(mark);
          mark.set.push(this);
          mark.from += mylen;
          if (mark.to != null) mark.to += mylen;
        }
      }
    },
    addMark: function(from, to, style, set) {
      set.push(this);
      if (this.marked == null) this.marked = [];
      this.marked.push({from: from, to: to, style: style, set: set});
      this.marked.sort(function(a, b){return (a.from || 0) - (b.from || 0);});
    },
    // Run the given mode's parser over a line, update the styles
    // array, which contains alternating fragments of text and CSS
    // classes.
    highlight: function(mode, state) {
      var stream = new StringStream(this.text), st = this.styles, pos = 0;
      var changed = false, curWord = st[0], prevWord;
      if (this.text == "" && mode.blankLine) mode.blankLine(state);
      while (!stream.eol()) {
        var style = mode.token(stream, state);
        var substr = this.text.slice(stream.start, stream.pos);
        stream.start = stream.pos;
        if (pos && st[pos-1] == style)
          st[pos-2] += substr;
        else if (substr) {
          if (!changed && (st[pos+1] != style || (pos && st[pos-2] != prevWord))) changed = true;
          st[pos++] = substr; st[pos++] = style;
          prevWord = curWord; curWord = st[pos];
        }
        // Give up when line is ridiculously long
        if (stream.pos > 5000) {
          st[pos++] = this.text.slice(stream.pos); st[pos++] = null;
          break;
        }
      }
      if (st.length != pos) {st.length = pos; changed = true;}
      if (pos && st[pos-2] != prevWord) changed = true;
      // Short lines with simple highlights return null, and are
      // counted as changed by the driver because they are likely to
      // highlight the same way in various contexts.
      return changed || (st.length < 5 && this.text.length < 10 ? null : false);
    },
    // Fetch the parser token for a given character. Useful for hacks
    // that want to inspect the mode state (say, for completion).
    getTokenAt: function(mode, state, ch) {
      var txt = this.text, stream = new StringStream(txt);
      while (stream.pos < ch && !stream.eol()) {
        stream.start = stream.pos;
        var style = mode.token(stream, state);
      }
      return {start: stream.start,
              end: stream.pos,
              string: stream.current(),
              className: style || null,
              state: state};
    },
    indentation: function() {return countColumn(this.text);},
    // Produces an HTML fragment for the line, taking selection,
    // marking, and highlighting into account.
    getHTML: function(sfrom, sto, includePre, endAt) {
      var html = [];
      if (includePre)
        html.push(this.className ? '<pre class="' + this.className + '">': "<pre>");
      function span(text, style) {
        if (!text) return;
        if (style) html.push('<span class="', style, '">', htmlEscape(text), "</span>");
        else html.push(htmlEscape(text));
      }
      var st = this.styles, allText = this.text, marked = this.marked;
      if (sfrom == sto) sfrom = null;
      var len = allText.length;
      if (endAt != null) len = Math.min(endAt, len);

      if (!allText && endAt == null)
        span(" ", sfrom != null && sto == null ? "CodeMirror-selected" : null);
      else if (!marked && sfrom == null)
        for (var i = 0, ch = 0; ch < len; i+=2) {
          var str = st[i], style = st[i+1], l = str.length;
          if (ch + l > len) str = str.slice(0, len - ch);
          ch += l;
          span(str, style && "cm-" + style);
        }
      else {
        var pos = 0, i = 0, text = "", style, sg = 0;
        var markpos = -1, mark = null;
        function nextMark() {
          if (marked) {
            markpos += 1;
            mark = (markpos < marked.length) ? marked[markpos] : null;
          }
        }
        nextMark();
        while (pos < len) {
          var upto = len;
          var extraStyle = "";
          if (sfrom != null) {
            if (sfrom > pos) upto = sfrom;
            else if (sto == null || sto > pos) {
              extraStyle = " CodeMirror-selected";
              if (sto != null) upto = Math.min(upto, sto);
            }
          }
          while (mark && mark.to != null && mark.to <= pos) nextMark();
          if (mark) {
            if (mark.from > pos) upto = Math.min(upto, mark.from);
            else {
              extraStyle += " " + mark.style;
              if (mark.to != null) upto = Math.min(upto, mark.to);
            }
          }
          for (;;) {
            var end = pos + text.length;
            var appliedStyle = style;
            if (extraStyle) appliedStyle = style ? style + extraStyle : extraStyle;
            span(end > upto ? text.slice(0, upto - pos) : text, appliedStyle);
            if (end >= upto) {text = text.slice(upto - pos); pos = upto; break;}
            pos = end;
            text = st[i++]; style = "cm-" + st[i++];
          }
        }
        if (sfrom != null && sto == null) span(" ", "CodeMirror-selected");
      }
      if (includePre) html.push("</pre>");
      return html.join("");
    }
  };
  // Utility used by replace and split above
  function copyStyles(from, to, source, dest) {
    for (var i = 0, pos = 0, state = 0; pos < to; i+=2) {
      var part = source[i], end = pos + part.length;
      if (state == 0) {
        if (end > from) dest.push(part.slice(from - pos, Math.min(part.length, to - pos)), source[i+1]);
        if (end >= from) state = 1;
      }
      else if (state == 1) {
        if (end > to) dest.push(part.slice(0, to - pos), source[i+1]);
        else dest.push(part, source[i+1]);
      }
      pos = end;
    }
  }

  // The history object 'chunks' changes that are made close together
  // and at almost the same time into bigger undoable units.
  function History() {
    this.time = 0;
    this.done = []; this.undone = [];
  }
  History.prototype = {
    addChange: function(start, added, old) {
      this.undone.length = 0;
      var time = +new Date, last = this.done[this.done.length - 1];
      if (time - this.time > 400 || !last ||
          last.start > start + added || last.start + last.added < start - last.added + last.old.length)
        this.done.push({start: start, added: added, old: old});
      else {
        var oldoff = 0;
        if (start < last.start) {
          for (var i = last.start - start - 1; i >= 0; --i)
            last.old.unshift(old[i]);
          last.added += last.start - start;
          last.start = start;
        }
        else if (last.start < start) {
          oldoff = start - last.start;
          added += oldoff;
        }
        for (var i = last.added - oldoff, e = old.length; i < e; ++i)
          last.old.push(old[i]);
        if (last.added < added) last.added = added;
      }
      this.time = time;
    }
  };

  function stopMethod() {e_stop(this);}
  // Ensure an event has a stop method.
  function addStop(event) {
    if (!event.stop) event.stop = stopMethod;
    return event;
  }

  function e_preventDefault(e) {
    if (e.preventDefault) e.preventDefault();
    else e.returnValue = false;
  }
  function e_stopPropagation(e) {
    if (e.stopPropagation) e.stopPropagation();
    else e.cancelBubble = true;
  }
  function e_stop(e) {e_preventDefault(e); e_stopPropagation(e);}
  function e_target(e) {return e.target || e.srcElement;}
  function e_button(e) {
    if (e.which) return e.which;
    else if (e.button & 1) return 1;
    else if (e.button & 2) return 3;
    else if (e.button & 4) return 2;
  }

  // Event handler registration. If disconnect is true, it'll return a
  // function that unregisters the handler.
  function connect(node, type, handler, disconnect) {
    function wrapHandler(event) {handler(event || window.event);}
    if (typeof node.addEventListener == "function") {
      node.addEventListener(type, wrapHandler, false);
      if (disconnect) return function() {node.removeEventListener(type, wrapHandler, false);};
    }
    else {
      node.attachEvent("on" + type, wrapHandler);
      if (disconnect) return function() {node.detachEvent("on" + type, wrapHandler);};
    }
  }

  function Delayed() {this.id = null;}
  Delayed.prototype = {set: function(ms, f) {clearTimeout(this.id); this.id = setTimeout(f, ms);}};

  // Some IE versions don't preserve whitespace when setting the
  // innerHTML of a PRE tag.
  var badInnerHTML = (function() {
    var pre = document.createElement("pre");
    pre.innerHTML = " "; return !pre.innerHTML;
  })();

  // Detect drag-and-drop
  var dragAndDrop = (function() {
    // IE8 has ondragstart and ondrop properties, but doesn't seem to
    // actually support ondragstart the way it's supposed to work.
    if (/MSIE [1-8]\b/.test(navigator.userAgent)) return false;
    var div = document.createElement('div');
    return "ondragstart" in div && "ondrop" in div;
  })();

  var gecko = /gecko\/\d{7}/i.test(navigator.userAgent);
  var ie = /MSIE \d/.test(navigator.userAgent);
  var safari = /Apple Computer/.test(navigator.vendor);

  var lineSep = "\n";
  // Feature-detect whether newlines in textareas are converted to \r\n
  (function () {
    var te = document.createElement("textarea");
    te.value = "foo\nbar";
    if (te.value.indexOf("\r") > -1) lineSep = "\r\n";
  }());

  var tabSize = 8;
  var mac = /Mac/.test(navigator.platform);
  var movementKeys = {};
  for (var i = 35; i <= 40; ++i)
    movementKeys[i] = movementKeys["c" + i] = true;

  // Counts the column offset in a string, taking tabs into account.
  // Used mostly to find indentation.
  function countColumn(string, end) {
    if (end == null) {
      end = string.search(/[^\s\u00a0]/);
      if (end == -1) end = string.length;
    }
    for (var i = 0, n = 0; i < end; ++i) {
      if (string.charAt(i) == "\t") n += tabSize - (n % tabSize);
      else ++n;
    }
    return n;
  }

  function computedStyle(elt) {
    if (elt.currentStyle) return elt.currentStyle;
    return window.getComputedStyle(elt, null);
  }
  // Find the position of an element by following the offsetParent chain.
  // If screen==true, it returns screen (rather than page) coordinates.
  function eltOffset(node, screen) {
    var doc = node.ownerDocument.body;
    var x = 0, y = 0, skipDoc = false;
    for (var n = node; n; n = n.offsetParent) {
      x += n.offsetLeft; y += n.offsetTop;
      if (screen && computedStyle(n).position == "fixed")
        skipDoc = true;
    }
    var e = screen && !skipDoc ? null : doc;
    for (var n = node.parentNode; n != e; n = n.parentNode)
      if (n.scrollLeft != null) { x -= n.scrollLeft; y -= n.scrollTop;}
    return {left: x, top: y};
  }
  // Get a node's text content.
  function eltText(node) {
    return node.textContent || node.innerText || node.nodeValue || "";
  }

  // Operations on {line, ch} objects.
  function posEq(a, b) {return a.line == b.line && a.ch == b.ch;}
  function posLess(a, b) {return a.line < b.line || (a.line == b.line && a.ch < b.ch);}
  function copyPos(x) {return {line: x.line, ch: x.ch};}

  var escapeElement = document.createElement("pre");
  function htmlEscape(str) {
    if (badTextContent) {
      escapeElement.innerHTML = "";
      escapeElement.appendChild(document.createTextNode(str));
    } else {
      escapeElement.textContent = str;
    }
    return escapeElement.innerHTML;
  }
  var badTextContent = htmlEscape("\t") != "\t";
  CodeMirror.htmlEscape = htmlEscape;

  // Used to position the cursor after an undo/redo by finding the
  // last edited character.
  function editEnd(from, to) {
    if (!to) return from ? from.length : 0;
    if (!from) return to.length;
    for (var i = from.length, j = to.length; i >= 0 && j >= 0; --i, --j)
      if (from.charAt(i) != to.charAt(j)) break;
    return j + 1;
  }

  function indexOf(collection, elt) {
    if (collection.indexOf) return collection.indexOf(elt);
    for (var i = 0, e = collection.length; i < e; ++i)
      if (collection[i] == elt) return i;
    return -1;
  }

  // See if "".split is the broken IE version, if so, provide an
  // alternative way to split lines.
  var splitLines, selRange, setSelRange;
  if ("\n\nb".split(/\n/).length != 3)
    splitLines = function(string) {
      var pos = 0, nl, result = [];
      while ((nl = string.indexOf("\n", pos)) > -1) {
        result.push(string.slice(pos, string.charAt(nl-1) == "\r" ? nl - 1 : nl));
        pos = nl + 1;
      }
      result.push(string.slice(pos));
      return result;
    };
  else
    splitLines = function(string){return string.split(/\r?\n/);};
  CodeMirror.splitLines = splitLines;

  // Sane model of finding and setting the selection in a textarea
  if (window.getSelection) {
    selRange = function(te) {
      try {return {start: te.selectionStart, end: te.selectionEnd};}
      catch(e) {return null;}
    };
    if (safari)
      // On Safari, selection set with setSelectionRange are in a sort
      // of limbo wrt their anchor. If you press shift-left in them,
      // the anchor is put at the end, and the selection expanded to
      // the left. If you press shift-right, the anchor ends up at the
      // front. This is not what CodeMirror wants, so it does a
      // spurious modify() call to get out of limbo.
      setSelRange = function(te, start, end) {
        if (start == end)
          te.setSelectionRange(start, end);
        else {
          te.setSelectionRange(start, end - 1);
          window.getSelection().modify("extend", "forward", "character");
        }
      };
    else
      setSelRange = function(te, start, end) {
        try {te.setSelectionRange(start, end);}
        catch(e) {} // Fails on Firefox when textarea isn't part of the document
      };
  }
  // IE model. Don't ask.
  else {
    selRange = function(te) {
      try {var range = te.ownerDocument.selection.createRange();}
      catch(e) {return null;}
      if (!range || range.parentElement() != te) return null;
      var val = te.value, len = val.length, localRange = te.createTextRange();
      localRange.moveToBookmark(range.getBookmark());
      var endRange = te.createTextRange();
      endRange.collapse(false);

      if (localRange.compareEndPoints("StartToEnd", endRange) > -1)
        return {start: len, end: len};

      var start = -localRange.moveStart("character", -len);
      for (var i = val.indexOf("\r"); i > -1 && i < start; i = val.indexOf("\r", i+1), start++) {}

      if (localRange.compareEndPoints("EndToEnd", endRange) > -1)
        return {start: start, end: len};

      var end = -localRange.moveEnd("character", -len);
      for (var i = val.indexOf("\r"); i > -1 && i < end; i = val.indexOf("\r", i+1), end++) {}
      return {start: start, end: end};
    };
    setSelRange = function(te, start, end) {
      var range = te.createTextRange();
      range.collapse(true);
      var endrange = range.duplicate();
      var newlines = 0, txt = te.value;
      for (var pos = txt.indexOf("\n"); pos > -1 && pos < start; pos = txt.indexOf("\n", pos + 1))
        ++newlines;
      range.move("character", start - newlines);
      for (; pos > -1 && pos < end; pos = txt.indexOf("\n", pos + 1))
        ++newlines;
      endrange.move("character", end - newlines);
      range.setEndPoint("EndToEnd", endrange);
      range.select();
    };
  }

  CodeMirror.defineMode("null", function() {
    return {token: function(stream) {stream.skipToEnd();}};
  });
  CodeMirror.defineMIME("text/plain", "null");

  return CodeMirror;
})();


CodeMirror.defineMode("clike", function (config, parserConfig) {
    var indentUnit = config.indentUnit,
        keywords = parserConfig.keywords || {},
        blockKeywords = parserConfig.blockKeywords || {},
        atoms = parserConfig.atoms || {},
        hooks = parserConfig.hooks || {},
        multiLineStrings = parserConfig.multiLineStrings;
    var isOperatorChar = /[+\-*&%=<>!?|\/]/;

    var curPunc;

    function tokenBase(stream, state) {
        var ch = stream.next();
        if (hooks[ch]) {
            var result = hooks[ch](stream, state);
            if (result !== false) return result;
        }
        if (ch == '"' || ch == "'") {
            state.tokenize = tokenString(ch);
            return state.tokenize(stream, state);
        }
        if (/[\[\]{}\(\),;\:\.]/.test(ch)) {
            curPunc = ch;
            return null
        }
        if (/\d/.test(ch)) {
            stream.eatWhile(/[\w\.]/);
            return "number";
        }
        if (ch == "/") {
            if (stream.eat("*")) {
                state.tokenize = tokenComment;
                return tokenComment(stream, state);
            }
            if (stream.eat("/")) {
                stream.skipToEnd();
                return "comment";
            }
        }
        if (isOperatorChar.test(ch)) {
            stream.eatWhile(isOperatorChar);
            return "operator";
        }
        stream.eatWhile(/[\w\$_]/);
        var cur = stream.current();
        if (keywords.propertyIsEnumerable(cur)) {
            if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
            return "keyword";
        }
        if (atoms.propertyIsEnumerable(cur)) return "atom";
        return "word";
    }

    function tokenString(quote) {
        return function (stream, state) {
            var escaped = false, next, end = false;
            while ((next = stream.next()) != null) {
                if (next == quote && !escaped) { end = true; break; }
                escaped = !escaped && next == "\\";
            }
            if (end || !(escaped || multiLineStrings))
                state.tokenize = tokenBase;
            return "string";
        };
    }

    function tokenComment(stream, state) {
        var maybeEnd = false, ch;
        while (ch = stream.next()) {
            if (ch == "/" && maybeEnd) {
                state.tokenize = tokenBase;
                break;
            }
            maybeEnd = (ch == "*");
        }
        return "comment";
    }

    function Context(indented, column, type, align, prev) {
        this.indented = indented;
        this.column = column;
        this.type = type;
        this.align = align;
        this.prev = prev;
    }
    function pushContext(state, col, type) {
        return state.context = new Context(state.indented, col, type, null, state.context);
    }
    function popContext(state) {
        var t = state.context.type;
        if (t == ")" || t == "]" || t == "}")
            state.indented = state.context.indented;
        return state.context = state.context.prev;
    }

    // Interface

    return {
        startState: function (basecolumn) {
            return {
                tokenize: null,
                context: new Context((basecolumn || 0) - indentUnit, 0, "top", false),
                indented: 0,
                startOfLine: true
            };
        },

        token: function (stream, state) {
            var ctx = state.context;
            if (stream.sol()) {
                if (ctx.align == null) ctx.align = false;
                state.indented = stream.indentation();
                state.startOfLine = true;
            }
            if (stream.eatSpace()) return null;
            curPunc = null;
            var style = (state.tokenize || tokenBase)(stream, state);
            if (style == "comment" || style == "meta") return style;
            if (ctx.align == null) ctx.align = true;

            if ((curPunc == ";" || curPunc == ":") && ctx.type == "statement") popContext(state);
            else if (curPunc == "{") pushContext(state, stream.column(), "}");
            else if (curPunc == "[") pushContext(state, stream.column(), "]");
            else if (curPunc == "(") pushContext(state, stream.column(), ")");
            else if (curPunc == "}") {
                while (ctx.type == "statement") ctx = popContext(state);
                if (ctx.type == "}") ctx = popContext(state);
                while (ctx.type == "statement") ctx = popContext(state);
            }
            else if (curPunc == ctx.type) popContext(state);
            else if (ctx.type == "}" || ctx.type == "top" || (ctx.type == "statement" && curPunc == "newstatement"))
                pushContext(state, stream.column(), "statement");
            state.startOfLine = false;
            return style;
        },

        indent: function (state, textAfter) {
            if (state.tokenize != tokenBase && state.tokenize != null) return 0;
            var firstChar = textAfter && textAfter.charAt(0), ctx = state.context, closing = firstChar == ctx.type;
            if (ctx.type == "statement") return ctx.indented + (firstChar == "{" ? 0 : indentUnit);
            else if (ctx.align) return ctx.column + (closing ? 0 : 1);
            else return ctx.indented + (closing ? 0 : indentUnit);
        },

        electricChars: "{}"
    };
});

(function () {
    function words(str) {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
        return obj;
    }
    var cKeywords = "auto if break int case long char register continue return default short do sizeof " +
      "double static else struct entry switch extern typedef float union for unsigned " +
      "goto while enum void const signed volatile";

    function cppHook(stream, state) {
        if (!state.startOfLine) return false;
        stream.skipToEnd();
        return "meta";
    }

    // C#-style strings where "" escapes a quote.
    function tokenAtString(stream, state) {
        var next;
        while ((next = stream.next()) != null) {
            if (next == '"' && !stream.eat('"')) {
                state.tokenize = null;
                break;
            }
        }
        return "string";
    }

    CodeMirror.defineMIME("text/x-csrc", {
        name: "clike",
        keywords: words(cKeywords),
        blockKeywords: words("case do else for if switch while struct"),
        atoms: words("null"),
        hooks: { "#": cppHook }
    });
    CodeMirror.defineMIME("text/x-c++src", {
        name: "clike",
        keywords: words(cKeywords + " asm dynamic_cast namespace reinterpret_cast try bool explicit new " +
                        "static_cast typeid catch operator template typename class friend private " +
                        "this using const_cast inline public throw virtual delete mutable protected " +
                        "wchar_t"),
        blockKeywords: words("catch class do else finally for if struct switch try while"),
        atoms: words("true false null"),
        hooks: { "#": cppHook }
    });
    CodeMirror.defineMIME("text/x-java", {
        name: "clike",
        keywords: words("abstract assert boolean break byte case catch char class const continue default " +
                        "do double else enum extends final finally float for goto if implements import " +
                        "instanceof int interface long native new package private protected public " +
                        "return short static strictfp super switch synchronized this throw throws transient " +
                        "try void volatile while"),
        blockKeywords: words("catch class do else finally for if switch try while"),
        atoms: words("true false null"),
        hooks: {
            "@": function (stream, state) {
                stream.eatWhile(/[\w\$_]/);
                return "meta";
            }
        }
    });
    CodeMirror.defineMIME("text/x-csharp", {
        name: "clike",
        keywords: words("abstract as base bool break byte case catch char checked class const continue decimal" +
                        " default delegate do double else enum event explicit extern finally fixed float for" +
                        " foreach goto if implicit in int interface internal is lock long namespace new object" +
                        " operator out override params private protected public readonly ref return sbyte sealed short" +
                        " sizeof stackalloc static string struct switch this throw try typeof uint ulong unchecked" +
                        " unsafe ushort using virtual void volatile while add alias ascending descending dynamic from get" +
                        " global group into join let orderby partial remove select set value var yield"),
        blockKeywords: words("catch class do else finally for foreach if struct switch try while"),
        atoms: words("true false null"),
        hooks: {
            "@": function (stream, state) {
                if (stream.eat('"')) {
                    state.tokenize = tokenAtString;
                    return tokenAtString(stream, state);
                }
                stream.eatWhile(/[\w\$_]/);
                return "meta";
            }
        }
    });
    CodeMirror.defineMIME("text/x-groovy", {
        name: "clike",
        keywords: words("abstract as assert boolean break byte case catch char class const continue def default " +
                        "do double else enum extends final finally float for goto if implements import " +
                        "in instanceof int interface long native new package property private protected public " +
                        "return short static strictfp super switch synchronized this throw throws transient " +
                        "try void volatile while"),
        atoms: words("true false null"),
        hooks: {
            "@": function (stream, state) {
                stream.eatWhile(/[\w\$_]/);
                return "meta";
            }
        }
    });
}());

CodeMirror.defineMode("r", function (config) {
    function wordObj(str) {
        var words = str.split(" "), res = {};
        for (var i = 0; i < words.length; ++i) res[words[i]] = true;
        return res;
    }
    var atoms = wordObj("NULL NA Inf NaN NA_integer_ NA_real_ NA_complex_ NA_character_");
    var builtins = wordObj("list quote bquote eval return call parse deparse");
    var keywords = wordObj("if else repeat while function for in next break");
    var blockkeywords = wordObj("if else repeat while function for");
    var opChars = /[+\-*\/^<>=!&|~$:]/;
    var curPunc;

    function tokenBase(stream, state) {
        curPunc = null;
        var ch = stream.next();
        if (ch == "#") {
            stream.skipToEnd();
            return "comment";
        } else if (ch == "0" && stream.eat("x")) {
            stream.eatWhile(/[\da-f]/i);
            return "number";
        } else if (ch == "." && stream.eat(/\d/)) {
            stream.match(/\d*(?:e[+\-]?\d+)?/);
            return "number";
        } else if (/\d/.test(ch)) {
            stream.match(/\d*(?:\.\d+)?(?:e[+\-]\d+)?L?/);
            return "number";
        } else if (ch == "'" || ch == '"') {
            state.tokenize = tokenString(ch);
            return "string";
        } else if (ch == "." && stream.match(/.[.\d]+/)) {
            return "keyword";
        } else if (/[\w\.]/.test(ch) && ch != "_") {
            stream.eatWhile(/[\w\.]/);
            var word = stream.current();
            if (atoms.propertyIsEnumerable(word)) return "atom";
            if (keywords.propertyIsEnumerable(word)) {
                if (blockkeywords.propertyIsEnumerable(word)) curPunc = "block";
                return "keyword";
            }
            if (builtins.propertyIsEnumerable(word)) return "builtin";
            return "variable";
        } else if (ch == "%") {
            if (stream.skipTo("%")) stream.next();
            return "variable-2";
        } else if (ch == "<" && stream.eat("-")) {
            return "arrow";
        } else if (ch == "=" && state.ctx.argList) {
            return "arg-is";
        } else if (opChars.test(ch)) {
            if (ch == "$") return "dollar";
            stream.eatWhile(opChars);
            return "operator";
        } else if (/[\(\){}\[\];]/.test(ch)) {
            curPunc = ch;
            if (ch == ";") return "semi";
            return null;
        } else {
            return null;
        }
    }

    function tokenString(quote) {
        return function (stream, state) {
            if (stream.eat("\\")) {
                var ch = stream.next();
                if (ch == "x") stream.match(/^[a-f0-9]{2}/i);
                else if ((ch == "u" || ch == "U") && stream.eat("{") && stream.skipTo("}")) stream.next();
                else if (ch == "u") stream.match(/^[a-f0-9]{4}/i);
                else if (ch == "U") stream.match(/^[a-f0-9]{8}/i);
                else if (/[0-7]/.test(ch)) stream.match(/^[0-7]{1,2}/);
                return "string-2";
            } else {
                var next;
                while ((next = stream.next()) != null) {
                    if (next == quote) { state.tokenize = tokenBase; break; }
                    if (next == "\\") { stream.backUp(1); break; }
                }
                return "string";
            }
        };
    }

    function push(state, type, stream) {
        state.ctx = {
            type: type,
            indent: state.indent,
            align: null,
            column: stream.column(),
            prev: state.ctx
        };
    }
    function pop(state) {
        state.indent = state.ctx.indent;
        state.ctx = state.ctx.prev;
    }

    return {
        startState: function (base) {
            return {
                tokenize: tokenBase,
                ctx: {
                    type: "top",
                    indent: -config.indentUnit,
                    align: false
                },
                indent: 0,
                afterIdent: false
            };
        },

        token: function (stream, state) {
            if (stream.sol()) {
                if (state.ctx.align == null) state.ctx.align = false;
                state.indent = stream.indentation();
            }
            if (stream.eatSpace()) return null;
            var style = state.tokenize(stream, state);
            if (style != "comment" && state.ctx.align == null) state.ctx.align = true;

            var ctype = state.ctx.type;
            if ((curPunc == ";" || curPunc == "{" || curPunc == "}") && ctype == "block") pop(state);
            if (curPunc == "{") push(state, "}", stream);
            else if (curPunc == "(") {
                push(state, ")", stream);
                if (state.afterIdent) state.ctx.argList = true;
            }
            else if (curPunc == "[") push(state, "]", stream);
            else if (curPunc == "block") push(state, "block", stream);
            else if (curPunc == ctype) pop(state);
            state.afterIdent = style == "variable" || style == "keyword";
            return style;
        },

        indent: function (state, textAfter) {
            if (state.tokenize != tokenBase) return 0;
            var firstChar = textAfter && textAfter.charAt(0), ctx = state.ctx,
                closing = firstChar == ctx.type;
            if (ctx.type == "block") return ctx.indent + (firstChar == "{" ? 0 : config.indentUnit);
            else if (ctx.align) return ctx.column + (closing ? 0 : 1);
            else return ctx.indent + (closing ? 0 : config.indentUnit);
        }
    };
});

CodeMirror.defineMIME("text/x-rsrc", "r");

CodeMirror.defineMode("python", function (conf, parserConf) {
    var ERRORCLASS = 'error';

    function wordRegexp(words) {
        return new RegExp("^((" + words.join(")|(") + "))\\b");
    }

    var singleOperators = new RegExp("^[\\+\\-\\*/%&|\\^~<>!]");
    var singleDelimiters = new RegExp('^[\\(\\)\\[\\]\\{\\}@,:`=;\\.]');
    var doubleOperators = new RegExp("^((==)|(!=)|(<=)|(>=)|(<>)|(<<)|(>>)|(//)|(\\*\\*))");
    var doubleDelimiters = new RegExp("^((\\+=)|(\\-=)|(\\*=)|(%=)|(/=)|(&=)|(\\|=)|(\\^=))");
    var tripleDelimiters = new RegExp("^((//=)|(>>=)|(<<=)|(\\*\\*=))");
    var identifiers = new RegExp("^[_A-Za-z][_A-Za-z0-9]*");

    var wordOperators = wordRegexp(['and', 'or', 'not', 'is', 'in']);
    var commonkeywords = ['as', 'assert', 'break', 'class', 'continue',
                          'def', 'del', 'elif', 'else', 'except', 'finally',
                          'for', 'from', 'global', 'if', 'import',
                          'lambda', 'pass', 'raise', 'return',
                          'try', 'while', 'with', 'yield'];
    var commontypes = ['bool', 'classmethod', 'complex', 'dict', 'enumerate',
                       'float', 'frozenset', 'int', 'list', 'object',
                       'property', 'reversed', 'set', 'slice', 'staticmethod',
                       'str', 'super', 'tuple', 'type'];
    var py2 = {
        'types': ['basestring', 'buffer', 'file', 'long', 'unicode',
                  'xrange'],
        'keywords': ['exec', 'print']
    };
    var py3 = {
        'types': ['bytearray', 'bytes', 'filter', 'map', 'memoryview',
                  'open', 'range', 'zip'],
        'keywords': ['nonlocal']
    };

    if (!!parserConf.version && parseInt(parserConf.version, 10) === 3) {
        commonkeywords = commonkeywords.concat(py3.keywords);
        commontypes = commontypes.concat(py3.types);
        var stringPrefixes = new RegExp("^(([rb]|(br))?('{3}|\"{3}|['\"]))", "i");
    } else {
        commonkeywords = commonkeywords.concat(py2.keywords);
        commontypes = commontypes.concat(py2.types);
        var stringPrefixes = new RegExp("^(([rub]|(ur)|(br))?('{3}|\"{3}|['\"]))", "i");
    }
    var keywords = wordRegexp(commonkeywords);
    var types = wordRegexp(commontypes);

    var indentInfo = null;

    // tokenizers
    function tokenBase(stream, state) {
        // Handle scope changes
        if (stream.sol()) {
            var scopeOffset = state.scopes[0].offset;
            if (stream.eatSpace()) {
                var lineOffset = stream.indentation();
                if (lineOffset > scopeOffset) {
                    indentInfo = 'indent';
                } else if (lineOffset < scopeOffset) {
                    indentInfo = 'dedent';
                }
                return null;
            } else {
                if (scopeOffset > 0) {
                    dedent(stream, state);
                }
            }
        }
        if (stream.eatSpace()) {
            return null;
        }

        var ch = stream.peek();

        // Handle Comments
        if (ch === '#') {
            stream.skipToEnd();
            return 'comment';
        }

        // Handle Number Literals
        if (stream.match(/^[0-9\.]/, false)) {
            var floatLiteral = false;
            // Floats
            if (stream.match(/^\d*\.\d+(e[\+\-]?\d+)?/i)) { floatLiteral = true; }
            if (stream.match(/^\d+\.\d*/)) { floatLiteral = true; }
            if (stream.match(/^\.\d+/)) { floatLiteral = true; }
            if (floatLiteral) {
                // Float literals may be "imaginary"
                stream.eat(/J/i);
                return 'number';
            }
            // Integers
            var intLiteral = false;
            // Hex
            if (stream.match(/^0x[0-9a-f]+/i)) { intLiteral = true; }
            // Binary
            if (stream.match(/^0b[01]+/i)) { intLiteral = true; }
            // Octal
            if (stream.match(/^0o[0-7]+/i)) { intLiteral = true; }
            // Decimal
            if (stream.match(/^[1-9]\d*(e[\+\-]?\d+)?/)) {
                // Decimal literals may be "imaginary"
                stream.eat(/J/i);
                // TODO - Can you have imaginary longs?
                intLiteral = true;
            }
            // Zero by itself with no other piece of number.
            if (stream.match(/^0(?![\dx])/i)) { intLiteral = true; }
            if (intLiteral) {
                // Integer literals may be "long"
                stream.eat(/L/i);
                return 'number';
            }
        }

        // Handle Strings
        if (stream.match(stringPrefixes)) {
            state.tokenize = tokenStringFactory(stream.current());
            return state.tokenize(stream, state);
        }

        // Handle operators and Delimiters
        if (stream.match(tripleDelimiters) || stream.match(doubleDelimiters)) {
            return null;
        }
        if (stream.match(doubleOperators)
            || stream.match(singleOperators)
            || stream.match(wordOperators)) {
            return 'operator';
        }
        if (stream.match(singleDelimiters)) {
            return null;
        }

        if (stream.match(types)) {
            return 'builtin';
        }

        if (stream.match(keywords)) {
            return 'keyword';
        }

        if (stream.match(identifiers)) {
            return 'variable';
        }

        // Handle non-detected items
        stream.next();
        return ERRORCLASS;
    }

    function tokenStringFactory(delimiter) {
        while ('rub'.indexOf(delimiter.charAt(0).toLowerCase()) >= 0) {
            delimiter = delimiter.substr(1);
        }
        var singleline = delimiter.length == 1;
        var OUTCLASS = 'string';

        return function tokenString(stream, state) {
            while (!stream.eol()) {
                stream.eatWhile(/[^'"\\]/);
                if (stream.eat('\\')) {
                    stream.next();
                    if (singleline && stream.eol()) {
                        return OUTCLASS;
                    }
                } else if (stream.match(delimiter)) {
                    state.tokenize = tokenBase;
                    return OUTCLASS;
                } else {
                    stream.eat(/['"]/);
                }
            }
            if (singleline) {
                if (parserConf.singleLineStringErrors) {
                    return ERRORCLASS;
                } else {
                    state.tokenize = tokenBase;
                }
            }
            return OUTCLASS;
        };
    }

    function indent(stream, state, type) {
        type = type || 'py';
        var indentUnit = 0;
        if (type === 'py') {
            for (var i = 0; i < state.scopes.length; ++i) {
                if (state.scopes[i].type === 'py') {
                    indentUnit = state.scopes[i].offset + conf.indentUnit;
                    break;
                }
            }
        } else {
            indentUnit = stream.column() + stream.current().length;
        }
        state.scopes.unshift({
            offset: indentUnit,
            type: type
        });
    }

    function dedent(stream, state) {
        if (state.scopes.length == 1) return;
        if (state.scopes[0].type === 'py') {
            var _indent = stream.indentation();
            var _indent_index = -1;
            for (var i = 0; i < state.scopes.length; ++i) {
                if (_indent === state.scopes[i].offset) {
                    _indent_index = i;
                    break;
                }
            }
            if (_indent_index === -1) {
                return true;
            }
            while (state.scopes[0].offset !== _indent) {
                state.scopes.shift();
            }
            return false
        } else {
            state.scopes.shift();
            return false;
        }
    }

    function tokenLexer(stream, state) {
        indentInfo = null;
        var style = state.tokenize(stream, state);
        var current = stream.current();

        // Handle '.' connected identifiers
        if (current === '.') {
            style = state.tokenize(stream, state);
            current = stream.current();
            if (style === 'variable') {
                return 'variable';
            } else {
                return ERRORCLASS;
            }
        }

        // Handle decorators
        if (current === '@') {
            style = state.tokenize(stream, state);
            current = stream.current();
            if (style === 'variable'
                || current === '@staticmethod'
                || current === '@classmethod') {
                return 'meta';
            } else {
                return ERRORCLASS;
            }
        }

        // Handle scope changes.
        if (current === 'pass' || current === 'return') {
            state.dedent += 1;
        }
        if ((current === ':' && !state.lambda && state.scopes[0].type == 'py')
            || indentInfo === 'indent') {
            indent(stream, state);
        }
        var delimiter_index = '[({'.indexOf(current);
        if (delimiter_index !== -1) {
            indent(stream, state, '])}'.slice(delimiter_index, delimiter_index + 1));
        }
        if (indentInfo === 'dedent') {
            if (dedent(stream, state)) {
                return ERRORCLASS;
            }
        }
        delimiter_index = '])}'.indexOf(current);
        if (delimiter_index !== -1) {
            if (dedent(stream, state)) {
                return ERRORCLASS;
            }
        }
        if (state.dedent > 0 && stream.eol() && state.scopes[0].type == 'py') {
            if (state.scopes.length > 1) state.scopes.shift();
            state.dedent -= 1;
        }

        return style;
    }

    var external = {
        startState: function (basecolumn) {
            return {
                tokenize: tokenBase,
                scopes: [{ offset: basecolumn || 0, type: 'py' }],
                lastToken: null,
                lambda: false,
                dedent: 0
            };
        },

        token: function (stream, state) {
            var style = tokenLexer(stream, state);

            state.lastToken = { style: style, content: stream.current() };

            if (stream.eol() && stream.lambda) {
                state.lambda = false;
            }

            return style;
        },

        indent: function (state, textAfter) {
            if (state.tokenize != tokenBase) {
                return 0;
            }

            return state.scopes[0].offset;
        }

    };
    return external;
});

CodeMirror.defineMIME("text/x-python", "python");
CodeMirror.defineMode("plsql", function (config, parserConfig) {
    var indentUnit = config.indentUnit,
        keywords = parserConfig.keywords,
        functions = parserConfig.functions,
        types = parserConfig.types,
        sqlplus = parserConfig.sqlplus,
        multiLineStrings = parserConfig.multiLineStrings;
    var isOperatorChar = /[+\-*&%=<>!?:\/|]/;
    function chain(stream, state, f) {
        state.tokenize = f;
        return f(stream, state);
    }

    var type;
    function ret(tp, style) {
        type = tp;
        return style;
    }

    function tokenBase(stream, state) {
        var ch = stream.next();
        // start of string?
        if (ch == '"' || ch == "'")
            return chain(stream, state, tokenString(ch));
            // is it one of the special signs []{}().,;? Seperator?
        else if (/[\[\]{}\(\),;\.]/.test(ch))
            return ret(ch);
            // start of a number value?
        else if (/\d/.test(ch)) {
            stream.eatWhile(/[\w\.]/);
            return ret("number", "number");
        }
            // multi line comment or simple operator?
        else if (ch == "/") {
            if (stream.eat("*")) {
                return chain(stream, state, tokenComment);
            }
            else {
                stream.eatWhile(isOperatorChar);
                return ret("operator", "operator");
            }
        }
            // single line comment or simple operator?
        else if (ch == "-") {
            if (stream.eat("-")) {
                stream.skipToEnd();
                return ret("comment", "comment");
            }
            else {
                stream.eatWhile(isOperatorChar);
                return ret("operator", "operator");
            }
        }
            // pl/sql variable?
        else if (ch == "@" || ch == "$") {
            stream.eatWhile(/[\w\d\$_]/);
            return ret("word", "variable");
        }
            // is it a operator?
        else if (isOperatorChar.test(ch)) {
            stream.eatWhile(isOperatorChar);
            return ret("operator", "operator");
        }
        else {
            // get the whole word
            stream.eatWhile(/[\w\$_]/);
            // is it one of the listed keywords?
            if (keywords && keywords.propertyIsEnumerable(stream.current().toLowerCase())) return ret("keyword", "keyword");
            // is it one of the listed functions?
            if (functions && functions.propertyIsEnumerable(stream.current().toLowerCase())) return ret("keyword", "builtin");
            // is it one of the listed types?
            if (types && types.propertyIsEnumerable(stream.current().toLowerCase())) return ret("keyword", "variable-2");
            // is it one of the listed sqlplus keywords?
            if (sqlplus && sqlplus.propertyIsEnumerable(stream.current().toLowerCase())) return ret("keyword", "variable-3");
            // default: just a "word"
            return ret("word", "plsql-word");
        }
    }

    function tokenString(quote) {
        return function (stream, state) {
            var escaped = false, next, end = false;
            while ((next = stream.next()) != null) {
                if (next == quote && !escaped) { end = true; break; }
                escaped = !escaped && next == "\\";
            }
            if (end || !(escaped || multiLineStrings))
                state.tokenize = tokenBase;
            return ret("string", "plsql-string");
        };
    }

    function tokenComment(stream, state) {
        var maybeEnd = false, ch;
        while (ch = stream.next()) {
            if (ch == "/" && maybeEnd) {
                state.tokenize = tokenBase;
                break;
            }
            maybeEnd = (ch == "*");
        }
        return ret("comment", "plsql-comment");
    }

    // Interface

    return {
        startState: function (basecolumn) {
            return {
                tokenize: tokenBase,
                startOfLine: true
            };
        },

        token: function (stream, state) {
            if (stream.eatSpace()) return null;
            var style = state.tokenize(stream, state);
            return style;
        }
    };
});

(function () {
    function keywords(str) {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
        return obj;
    }
    var cKeywords = "abort accept access add all alter and any array arraylen as asc assert assign at attributes audit " +
          "authorization avg " +
          "base_table begin between binary_integer body boolean by " +
          "case cast char char_base check close cluster clusters colauth column comment commit compress connect " +
          "connected constant constraint crash create current currval cursor " +
          "data_base database date dba deallocate debugoff debugon decimal declare default definition delay delete " +
          "desc digits dispose distinct do drop " +
          "else elsif enable end entry escape exception exception_init exchange exclusive exists exit external " +
          "fast fetch file for force form from function " +
          "generic goto grant group " +
          "having " +
          "identified if immediate in increment index indexes indicator initial initrans insert interface intersect " +
          "into is " +
          "key " +
          "level library like limited local lock log logging long loop " +
          "master maxextents maxtrans member minextents minus mislabel mode modify multiset " +
          "new next no noaudit nocompress nologging noparallel not nowait number_base " +
          "object of off offline on online only open option or order out " +
          "package parallel partition pctfree pctincrease pctused pls_integer positive positiven pragma primary prior " +
          "private privileges procedure public " +
          "raise range raw read rebuild record ref references refresh release rename replace resource restrict return " +
          "returning reverse revoke rollback row rowid rowlabel rownum rows run " +
          "savepoint schema segment select separate session set share snapshot some space split sql start statement " +
          "storage subtype successful synonym " +
          "tabauth table tables tablespace task terminate then to trigger truncate type " +
          "union unique unlimited unrecoverable unusable update use using " +
          "validate value values variable view views " +
          "when whenever where while with work";

    var cFunctions = "abs acos add_months ascii asin atan atan2 average " +
          "bfilename " +
          "ceil chartorowid chr concat convert cos cosh count " +
          "decode deref dual dump dup_val_on_index " +
          "empty error exp " +
          "false floor found " +
          "glb greatest " +
          "hextoraw " +
          "initcap instr instrb isopen " +
          "last_day least lenght lenghtb ln lower lpad ltrim lub " +
          "make_ref max min mod months_between " +
          "new_time next_day nextval nls_charset_decl_len nls_charset_id nls_charset_name nls_initcap nls_lower " +
          "nls_sort nls_upper nlssort no_data_found notfound null nvl " +
          "others " +
          "power " +
          "rawtohex reftohex round rowcount rowidtochar rpad rtrim " +
          "sign sin sinh soundex sqlcode sqlerrm sqrt stddev substr substrb sum sysdate " +
          "tan tanh to_char to_date to_label to_multi_byte to_number to_single_byte translate true trunc " +
          "uid upper user userenv " +
          "variance vsize";

    var cTypes = "bfile blob " +
          "character clob " +
          "dec " +
          "float " +
          "int integer " +
          "mlslabel " +
          "natural naturaln nchar nclob number numeric nvarchar2 " +
          "real rowtype " +
          "signtype smallint string " +
          "varchar varchar2";

    var cSqlplus = "appinfo arraysize autocommit autoprint autorecovery autotrace " +
          "blockterminator break btitle " +
          "cmdsep colsep compatibility compute concat copycommit copytypecheck " +
          "define describe " +
          "echo editfile embedded escape exec execute " +
          "feedback flagger flush " +
          "heading headsep " +
          "instance " +
          "linesize lno loboffset logsource long longchunksize " +
          "markup " +
          "native newpage numformat numwidth " +
          "pagesize pause pno " +
          "recsep recsepchar release repfooter repheader " +
          "serveroutput shiftinout show showmode size spool sqlblanklines sqlcase sqlcode sqlcontinue sqlnumber " +
          "sqlpluscompatibility sqlprefix sqlprompt sqlterminator suffix " +
          "tab term termout time timing trimout trimspool ttitle " +
          "underline " +
          "verify version " +
          "wrap";

    CodeMirror.defineMIME("text/x-plsql", {
        name: "plsql",
        keywords: keywords(cKeywords),
        functions: keywords(cFunctions),
        types: keywords(cTypes),
        sqlplus: keywords(cSqlplus)
    });
}());

(function () {
    function keywords(str) {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
        return obj;
    }
    function heredoc(delim) {
        return function (stream, state) {
            if (stream.match(delim)) state.tokenize = null;
            else stream.skipToEnd();
            return "string";
        }
    }
    var phpConfig = {
        name: "clike",
        keywords: keywords("abstract and array as break case catch cfunction class clone const continue declare " +
                           "default do else elseif enddeclare endfor endforeach endif endswitch endwhile extends " +
                           "final for foreach function global goto if implements interface instanceof namespace " +
                           "new or private protected public static switch throw try use var while xor return" +
                           "die echo empty exit eval include include_once isset list require require_once print unset"),
        blockKeywords: keywords("catch do else elseif for foreach if switch try while"),
        atoms: keywords("true false null TRUE FALSE NULL"),
        multiLineStrings: true,
        hooks: {
            "$": function (stream, state) {
                stream.eatWhile(/[\w\$_]/);
                return "variable-2";
            },
            "<": function (stream, state) {
                if (stream.match(/<</)) {
                    stream.eatWhile(/[\w\.]/);
                    state.tokenize = heredoc(stream.current().slice(3));
                    return state.tokenize(stream, state);
                }
                return false;
            },
            "#": function (stream, state) {
                stream.skipToEnd();
                return "comment";
            }
        }
    };

    CodeMirror.defineMode("php", function (config, parserConfig) {
        var htmlMode = CodeMirror.getMode(config, "text/html");
        var jsMode = CodeMirror.getMode(config, "text/javascript");
        var cssMode = CodeMirror.getMode(config, "text/css");
        var phpMode = CodeMirror.getMode(config, phpConfig);

        function dispatch(stream, state) { // TODO open PHP inside text/css
            if (state.curMode == htmlMode) {
                var style = htmlMode.token(stream, state.curState);
                if (style == "meta" && /^<\?/.test(stream.current())) {
                    state.curMode = phpMode;
                    state.curState = state.php;
                    state.curClose = /^\?>/;
                    state.mode = 'php';
                }
                else if (style == "tag" && stream.current() == ">" && state.curState.context) {
                    if (/^script$/i.test(state.curState.context.tagName)) {
                        state.curMode = jsMode;
                        state.curState = jsMode.startState(htmlMode.indent(state.curState, ""));
                        state.curClose = /^<\/\s*script\s*>/i;
                        state.mode = 'javascript';
                    }
                    else if (/^style$/i.test(state.curState.context.tagName)) {
                        state.curMode = cssMode;
                        state.curState = cssMode.startState(htmlMode.indent(state.curState, ""));
                        state.curClose = /^<\/\s*style\s*>/i;
                        state.mode = 'css';
                    }
                }
                return style;
            }
            else if (stream.match(state.curClose, false)) {
                state.curMode = htmlMode;
                state.curState = state.html;
                state.curClose = null;
                state.mode = 'html';
                return dispatch(stream, state);
            }
            else return state.curMode.token(stream, state.curState);
        }

        return {
            startState: function () {
                var html = htmlMode.startState();
                return {
                    html: html,
                    php: phpMode.startState(),
                    curMode: parserConfig.startOpen ? phpMode : htmlMode,
                    curState: parserConfig.startOpen ? phpMode.startState() : html,
                    curClose: parserConfig.startOpen ? /^\?>/ : null,
                    mode: parserConfig.startOpen ? 'php' : 'html'
                }
            },

            copyState: function (state) {
                var html = state.html, htmlNew = CodeMirror.copyState(htmlMode, html),
                    php = state.php, phpNew = CodeMirror.copyState(phpMode, php), cur;
                if (state.curState == html) cur = htmlNew;
                else if (state.curState == php) cur = phpNew;
                else cur = CodeMirror.copyState(state.curMode, state.curState);
                return { html: htmlNew, php: phpNew, curMode: state.curMode, curState: cur, curClose: state.curClose };
            },

            token: dispatch,

            indent: function (state, textAfter) {
                if ((state.curMode != phpMode && /^\s*<\//.test(textAfter)) ||
                    (state.curMode == phpMode && /^\?>/.test(textAfter)))
                    return htmlMode.indent(state.html, textAfter);
                return state.curMode.indent(state.curState, textAfter);
            },

            electricChars: "/{}:"
        }
    });
    CodeMirror.defineMIME("application/x-httpd-php", "php");
    CodeMirror.defineMIME("application/x-httpd-php-open", { name: "php", startOpen: true });
    CodeMirror.defineMIME("text/x-php", phpConfig);
})();

// CodeMirror2 mode/perl/perl.js (text/x-perl) beta 0.09 (2011-10-15)
// This is a part of CodeMirror from https://github.com/sabaca/CodeMirror_mode_perl (mail@sabaca.com)
CodeMirror.defineMode("perl", function (config, parserConfig) {
    // http://perldoc.perl.org
    var PERL = {				    	//   null - magic touch
        //   1 - keyword
        //   2 - def
        //   3 - atom
        //   4 - operator
        //   5 - variable-2 (predefined)
        //   [x,y] - x=1,2,3; y=must be defined if x{...}
        //	PERL operators
        '->': 4,
        '++': 4,
        '--': 4,
        '**': 4,
        //   ! ~ \ and unary + and -
        '=~': 4,
        '!~': 4,
        '*': 4,
        '/': 4,
        '%': 4,
        'x': 4,
        '+': 4,
        '-': 4,
        '.': 4,
        '<<': 4,
        '>>': 4,
        //   named unary operators
        '<': 4,
        '>': 4,
        '<=': 4,
        '>=': 4,
        'lt': 4,
        'gt': 4,
        'le': 4,
        'ge': 4,
        '==': 4,
        '!=': 4,
        '<=>': 4,
        'eq': 4,
        'ne': 4,
        'cmp': 4,
        '~~': 4,
        '&': 4,
        '|': 4,
        '^': 4,
        '&&': 4,
        '||': 4,
        '//': 4,
        '..': 4,
        '...': 4,
        '?': 4,
        ':': 4,
        '=': 4,
        '+=': 4,
        '-=': 4,
        '*=': 4,	//   etc. ???
        ',': 4,
        '=>': 4,
        '::': 4,
        //   list operators (rightward)
        'not': 4,
        'and': 4,
        'or': 4,
        'xor': 4,
        //	PERL predefined variables (I know, what this is a paranoid idea, but may be needed for people, who learn PERL, and for me as well, ...and may be for you?;)
        'BEGIN': [5, 1],
        'END': [5, 1],
        'PRINT': [5, 1],
        'PRINTF': [5, 1],
        'GETC': [5, 1],
        'READ': [5, 1],
        'READLINE': [5, 1],
        'DESTROY': [5, 1],
        'TIE': [5, 1],
        'TIEHANDLE': [5, 1],
        'UNTIE': [5, 1],
        'STDIN': 5,
        'STDIN_TOP': 5,
        'STDOUT': 5,
        'STDOUT_TOP': 5,
        'STDERR': 5,
        'STDERR_TOP': 5,
        '$ARG': 5,
        '$_': 5,
        '@ARG': 5,
        '@_': 5,
        '$LIST_SEPARATOR': 5,
        '$"': 5,
        '$PROCESS_ID': 5,
        '$PID': 5,
        '$$': 5,
        '$REAL_GROUP_ID': 5,
        '$GID': 5,
        '$(': 5,
        '$EFFECTIVE_GROUP_ID': 5,
        '$EGID': 5,
        '$)': 5,
        '$PROGRAM_NAME': 5,
        '$0': 5,
        '$SUBSCRIPT_SEPARATOR': 5,
        '$SUBSEP': 5,
        '$;': 5,
        '$REAL_USER_ID': 5,
        '$UID': 5,
        '$<': 5,
        '$EFFECTIVE_USER_ID': 5,
        '$EUID': 5,
        '$>': 5,
        '$a': 5,
        '$b': 5,
        '$COMPILING': 5,
        '$^C': 5,
        '$DEBUGGING': 5,
        '$^D': 5,
        '${^ENCODING}': 5,
        '$ENV': 5,
        '%ENV': 5,
        '$SYSTEM_FD_MAX': 5,
        '$^F': 5,
        '@F': 5,
        '${^GLOBAL_PHASE}': 5,
        '$^H': 5,
        '%^H': 5,
        '@INC': 5,
        '%INC': 5,
        '$INPLACE_EDIT': 5,
        '$^I': 5,
        '$^M': 5,
        '$OSNAME': 5,
        '$^O': 5,
        '${^OPEN}': 5,
        '$PERLDB': 5,
        '$^P': 5,
        '$SIG': 5,
        '%SIG': 5,
        '$BASETIME': 5,
        '$^T': 5,
        '${^TAINT}': 5,
        '${^UNICODE}': 5,
        '${^UTF8CACHE}': 5,
        '${^UTF8LOCALE}': 5,
        '$PERL_VERSION': 5,
        '$^V': 5,
        '${^WIN32_SLOPPY_STAT}': 5,
        '$EXECUTABLE_NAME': 5,
        '$^X': 5,
        '$1': 5,	// - regexp $1, $2...
        '$MATCH': 5,
        '$&': 5,
        '${^MATCH}': 5,
        '$PREMATCH': 5,
        '$`': 5,
        '${^PREMATCH}': 5,
        '$POSTMATCH': 5,
        "$'": 5,
        '${^POSTMATCH}': 5,
        '$LAST_PAREN_MATCH': 5,
        '$+': 5,
        '$LAST_SUBMATCH_RESULT': 5,
        '$^N': 5,
        '@LAST_MATCH_END': 5,
        '@+': 5,
        '%LAST_PAREN_MATCH': 5,
        '%+': 5,
        '@LAST_MATCH_START': 5,
        '@-': 5,
        '%LAST_MATCH_START': 5,
        '%-': 5,
        '$LAST_REGEXP_CODE_RESULT': 5,
        '$^R': 5,
        '${^RE_DEBUG_FLAGS}': 5,
        '${^RE_TRIE_MAXBUF}': 5,
        '$ARGV': 5,
        '@ARGV': 5,
        'ARGV': 5,
        'ARGVOUT': 5,
        '$OUTPUT_FIELD_SEPARATOR': 5,
        '$OFS': 5,
        '$,': 5,
        '$INPUT_LINE_NUMBER': 5,
        '$NR': 5,
        '$.': 5,
        '$INPUT_RECORD_SEPARATOR': 5,
        '$RS': 5,
        '$/': 5,
        '$OUTPUT_RECORD_SEPARATOR': 5,
        '$ORS': 5,
        '$\\': 5,
        '$OUTPUT_AUTOFLUSH': 5,
        '$|': 5,
        '$ACCUMULATOR': 5,
        '$^A': 5,
        '$FORMAT_FORMFEED': 5,
        '$^L': 5,
        '$FORMAT_PAGE_NUMBER': 5,
        '$%': 5,
        '$FORMAT_LINES_LEFT': 5,
        '$-': 5,
        '$FORMAT_LINE_BREAK_CHARACTERS': 5,
        '$:': 5,
        '$FORMAT_LINES_PER_PAGE': 5,
        '$=': 5,
        '$FORMAT_TOP_NAME': 5,
        '$^': 5,
        '$FORMAT_NAME': 5,
        '$~': 5,
        '${^CHILD_ERROR_NATIVE}': 5,
        '$EXTENDED_OS_ERROR': 5,
        '$^E': 5,
        '$EXCEPTIONS_BEING_CAUGHT': 5,
        '$^S': 5,
        '$WARNING': 5,
        '$^W': 5,
        '${^WARNING_BITS}': 5,
        '$OS_ERROR': 5,
        '$ERRNO': 5,
        '$!': 5,
        '%OS_ERROR': 5,
        '%ERRNO': 5,
        '%!': 5,
        '$CHILD_ERROR': 5,
        '$?': 5,
        '$EVAL_ERROR': 5,
        '$@': 5,
        '$OFMT': 5,
        '$#': 5,
        '$*': 5,
        '$ARRAY_BASE': 5,
        '$[': 5,
        '$OLD_PERL_VERSION': 5,
        '$]': 5,
        //	PERL blocks
        'if': [1, 1],
        elsif: [1, 1],
        'else': [1, 1],
        'while': [1, 1],
        unless: [1, 1],
        'for': [1, 1],
        foreach: [1, 1],
        //	PERL functions
        'abs': 1,	// - absolute value function
        accept: 1,	// - accept an incoming socket connect
        alarm: 1,	// - schedule a SIGALRM
        'atan2': 1,	// - arctangent of Y/X in the range -PI to PI
        bind: 1,	// - binds an address to a socket
        binmode: 1,	// - prepare binary files for I/O
        bless: 1,	// - create an object
        bootstrap: 1,	//
        'break': 1,	// - break out of a "given" block
        caller: 1,	// - get context of the current subroutine call
        chdir: 1,	// - change your current working directory
        chmod: 1,	// - changes the permissions on a list of files
        chomp: 1,	// - remove a trailing record separator from a string
        chop: 1,	// - remove the last character from a string
        chown: 1,	// - change the owership on a list of files
        chr: 1,	// - get character this number represents
        chroot: 1,	// - make directory new root for path lookups
        close: 1,	// - close file (or pipe or socket) handle
        closedir: 1,	// - close directory handle
        connect: 1,	// - connect to a remote socket
        'continue': [1, 1],	// - optional trailing block in a while or foreach
        'cos': 1,	// - cosine function
        crypt: 1,	// - one-way passwd-style encryption
        dbmclose: 1,	// - breaks binding on a tied dbm file
        dbmopen: 1,	// - create binding on a tied dbm file
        'default': 1,	//
        defined: 1,	// - test whether a value, variable, or function is defined
        'delete': 1,	// - deletes a value from a hash
        die: 1,	// - raise an exception or bail out
        'do': 1,	// - turn a BLOCK into a TERM
        dump: 1,	// - create an immediate core dump
        each: 1,	// - retrieve the next key/value pair from a hash
        endgrent: 1,	// - be done using group file
        endhostent: 1,	// - be done using hosts file
        endnetent: 1,	// - be done using networks file
        endprotoent: 1,	// - be done using protocols file
        endpwent: 1,	// - be done using passwd file
        endservent: 1,	// - be done using services file
        eof: 1,	// - test a filehandle for its end
        'eval': 1,	// - catch exceptions or compile and run code
        'exec': 1,	// - abandon this program to run another
        exists: 1,	// - test whether a hash key is present
        exit: 1,	// - terminate this program
        'exp': 1,	// - raise I to a power
        fcntl: 1,	// - file control system call
        fileno: 1,	// - return file descriptor from filehandle
        flock: 1,	// - lock an entire file with an advisory lock
        fork: 1,	// - create a new process just like this one
        format: 1,	// - declare a picture format with use by the write() function
        formline: 1,	// - internal function used for formats
        getc: 1,	// - get the next character from the filehandle
        getgrent: 1,	// - get next group record
        getgrgid: 1,	// - get group record given group user ID
        getgrnam: 1,	// - get group record given group name
        gethostbyaddr: 1,	// - get host record given its address
        gethostbyname: 1,	// - get host record given name
        gethostent: 1,	// - get next hosts record
        getlogin: 1,	// - return who logged in at this tty
        getnetbyaddr: 1,	// - get network record given its address
        getnetbyname: 1,	// - get networks record given name
        getnetent: 1,	// - get next networks record
        getpeername: 1,	// - find the other end of a socket connection
        getpgrp: 1,	// - get process group
        getppid: 1,	// - get parent process ID
        getpriority: 1,	// - get current nice value
        getprotobyname: 1,	// - get protocol record given name
        getprotobynumber: 1,	// - get protocol record numeric protocol
        getprotoent: 1,	// - get next protocols record
        getpwent: 1,	// - get next passwd record
        getpwnam: 1,	// - get passwd record given user login name
        getpwuid: 1,	// - get passwd record given user ID
        getservbyname: 1,	// - get services record given its name
        getservbyport: 1,	// - get services record given numeric port
        getservent: 1,	// - get next services record
        getsockname: 1,	// - retrieve the sockaddr for a given socket
        getsockopt: 1,	// - get socket options on a given socket
        given: 1,	//
        glob: 1,	// - expand filenames using wildcards
        gmtime: 1,	// - convert UNIX time into record or string using Greenwich time
        'goto': 1,	// - create spaghetti code
        grep: 1,	// - locate elements in a list test true against a given criterion
        hex: 1,	// - convert a string to a hexadecimal number
        'import': 1,	// - patch a module's namespace into your own
        index: 1,	// - find a substring within a string
        int: 1,	// - get the integer portion of a number
        ioctl: 1,	// - system-dependent device control system call
        'join': 1,	// - join a list into a string using a separator
        keys: 1,	// - retrieve list of indices from a hash
        kill: 1,	// - send a signal to a process or process group
        last: 1,	// - exit a block prematurely
        lc: 1,	// - return lower-case version of a string
        lcfirst: 1,	// - return a string with just the next letter in lower case
        length: 1,	// - return the number of bytes in a string
        'link': 1,	// - create a hard link in the filesytem
        listen: 1,	// - register your socket as a server
        local: 2,	// - create a temporary value for a global variable (dynamic scoping)
        localtime: 1,	// - convert UNIX time into record or string using local time
        lock: 1,	// - get a thread lock on a variable, subroutine, or method
        'log': 1,	// - retrieve the natural logarithm for a number
        lstat: 1,	// - stat a symbolic link
        m: null,	// - match a string with a regular expression pattern
        map: 1,	// - apply a change to a list to get back a new list with the changes
        mkdir: 1,	// - create a directory
        msgctl: 1,	// - SysV IPC message control operations
        msgget: 1,	// - get SysV IPC message queue
        msgrcv: 1,	// - receive a SysV IPC message from a message queue
        msgsnd: 1,	// - send a SysV IPC message to a message queue
        my: 2,	// - declare and assign a local variable (lexical scoping)
        'new': 1,	//
        next: 1,	// - iterate a block prematurely
        no: 1,	// - unimport some module symbols or semantics at compile time
        oct: 1,	// - convert a string to an octal number
        open: 1,	// - open a file, pipe, or descriptor
        opendir: 1,	// - open a directory
        ord: 1,	// - find a character's numeric representation
        our: 2,	// - declare and assign a package variable (lexical scoping)
        pack: 1,	// - convert a list into a binary representation
        'package': 1,	// - declare a separate global namespace
        pipe: 1,	// - open a pair of connected filehandles
        pop: 1,	// - remove the last element from an array and return it
        pos: 1,	// - find or set the offset for the last/next m//g search
        print: 1,	// - output a list to a filehandle
        printf: 1,	// - output a formatted list to a filehandle
        prototype: 1,	// - get the prototype (if any) of a subroutine
        push: 1,	// - append one or more elements to an array
        q: null,	// - singly quote a string
        qq: null,	// - doubly quote a string
        qr: null,	// - Compile pattern
        quotemeta: null,	// - quote regular expression magic characters
        qw: null,	// - quote a list of words
        qx: null,	// - backquote quote a string
        rand: 1,	// - retrieve the next pseudorandom number
        read: 1,	// - fixed-length buffered input from a filehandle
        readdir: 1,	// - get a directory from a directory handle
        readline: 1,	// - fetch a record from a file
        readlink: 1,	// - determine where a symbolic link is pointing
        readpipe: 1,	// - execute a system command and collect standard output
        recv: 1,	// - receive a message over a Socket
        redo: 1,	// - start this loop iteration over again
        ref: 1,	// - find out the type of thing being referenced
        rename: 1,	// - change a filename
        require: 1,	// - load in external functions from a library at runtime
        reset: 1,	// - clear all variables of a given name
        'return': 1,	// - get out of a function early
        reverse: 1,	// - flip a string or a list
        rewinddir: 1,	// - reset directory handle
        rindex: 1,	// - right-to-left substring search
        rmdir: 1,	// - remove a directory
        s: null,	// - replace a pattern with a string
        say: 1,	// - print with newline
        scalar: 1,	// - force a scalar context
        seek: 1,	// - reposition file pointer for random-access I/O
        seekdir: 1,	// - reposition directory pointer
        select: 1,	// - reset default output or do I/O multiplexing
        semctl: 1,	// - SysV semaphore control operations
        semget: 1,	// - get set of SysV semaphores
        semop: 1,	// - SysV semaphore operations
        send: 1,	// - send a message over a socket
        setgrent: 1,	// - prepare group file for use
        sethostent: 1,	// - prepare hosts file for use
        setnetent: 1,	// - prepare networks file for use
        setpgrp: 1,	// - set the process group of a process
        setpriority: 1,	// - set a process's nice value
        setprotoent: 1,	// - prepare protocols file for use
        setpwent: 1,	// - prepare passwd file for use
        setservent: 1,	// - prepare services file for use
        setsockopt: 1,	// - set some socket options
        shift: 1,	// - remove the first element of an array, and return it
        shmctl: 1,	// - SysV shared memory operations
        shmget: 1,	// - get SysV shared memory segment identifier
        shmread: 1,	// - read SysV shared memory
        shmwrite: 1,	// - write SysV shared memory
        shutdown: 1,	// - close down just half of a socket connection
        'sin': 1,	// - return the sine of a number
        sleep: 1,	// - block for some number of seconds
        socket: 1,	// - create a socket
        socketpair: 1,	// - create a pair of sockets
        'sort': 1,	// - sort a list of values
        splice: 1,	// - add or remove elements anywhere in an array
        'split': 1,	// - split up a string using a regexp delimiter
        sprintf: 1,	// - formatted print into a string
        'sqrt': 1,	// - square root function
        srand: 1,	// - seed the random number generator
        stat: 1,	// - get a file's status information
        state: 1,	// - declare and assign a state variable (persistent lexical scoping)
        study: 1,	// - optimize input data for repeated searches
        'sub': 1,	// - declare a subroutine, possibly anonymously
        'substr': 1,	// - get or alter a portion of a stirng
        symlink: 1,	// - create a symbolic link to a file
        syscall: 1,	// - execute an arbitrary system call
        sysopen: 1,	// - open a file, pipe, or descriptor
        sysread: 1,	// - fixed-length unbuffered input from a filehandle
        sysseek: 1,	// - position I/O pointer on handle used with sysread and syswrite
        system: 1,	// - run a separate program
        syswrite: 1,	// - fixed-length unbuffered output to a filehandle
        tell: 1,	// - get current seekpointer on a filehandle
        telldir: 1,	// - get current seekpointer on a directory handle
        tie: 1,	// - bind a variable to an object class
        tied: 1,	// - get a reference to the object underlying a tied variable
        time: 1,	// - return number of seconds since 1970
        times: 1,	// - return elapsed time for self and child processes
        tr: null,	// - transliterate a string
        truncate: 1,	// - shorten a file
        uc: 1,	// - return upper-case version of a string
        ucfirst: 1,	// - return a string with just the next letter in upper case
        umask: 1,	// - set file creation mode mask
        undef: 1,	// - remove a variable or function definition
        unlink: 1,	// - remove one link to a file
        unpack: 1,	// - convert binary structure into normal perl variables
        unshift: 1,	// - prepend more elements to the beginning of a list
        untie: 1,	// - break a tie binding to a variable
        use: 1,	// - load in a module at compile time
        utime: 1,	// - set a file's last access and modify times
        values: 1,	// - return a list of the values in a hash
        vec: 1,	// - test or set particular bits in a string
        wait: 1,	// - wait for any child process to die
        waitpid: 1,	// - wait for a particular child process to die
        wantarray: 1,	// - get void vs scalar vs list context of current subroutine call
        warn: 1,	// - print debugging info
        when: 1,	//
        write: 1,	// - print a picture record
        y: null
    };	// - transliterate a string

    var RXstyle = "string-2";
    var RXmodifiers = /[goseximacplud]/;		// NOTE: "m", "s", "y" and "tr" need to correct real modifiers for each regexp type

    function tokenChain(stream, state, chain, style, tail) {	// NOTE: chain.length > 2 is not working now (it's for s[...][...]geos;)
        state.chain = null;                               //                                                          12   3tail
        state.style = null;
        state.tail = null;
        state.tokenize = function (stream, state) {
            var e = false, c, i = 0;
            while (c = stream.next()) {
                if (c === chain[i] && !e) {
                    if (chain[++i] !== undefined) {
                        state.chain = chain[i];
                        state.style = style;
                        state.tail = tail
                    }
                    else if (tail)
                        stream.eatWhile(tail);
                    state.tokenize = tokenPerl;
                    return style
                }
                e = !e && c == "\\"
            }
            return style
        };
        return state.tokenize(stream, state)
    }

    function tokenSOMETHING(stream, state, string) {
        state.tokenize = function (stream, state) {
            if (stream.string == string)
                state.tokenize = tokenPerl;
            stream.skipToEnd();
            return "string"
        };
        return state.tokenize(stream, state)
    }

    function tokenPerl(stream, state) {
        if (stream.eatSpace())
            return null;
        if (state.chain)
            return tokenChain(stream, state, state.chain, state.style, state.tail);
        if (stream.match(/^\-?[\d\.]/, false))
            if (stream.match(/^(\-?(\d*\.\d+(e[+-]?\d+)?|\d+\.\d*)|0x[\da-fA-F]+|0b[01]+|\d+(e[+-]?\d+)?)/))
                return 'number';
        if (stream.match(/^<<(?=\w)/)) {			// NOTE: <<SOMETHING\n...\nSOMETHING\n
            stream.eatWhile(/\w/);
            return tokenSOMETHING(stream, state, stream.current().substr(2))
        }
        if (stream.sol() && stream.match(/^\=item(?!\w)/)) {// NOTE: \n=item...\n=cut\n
            return tokenSOMETHING(stream, state, '=cut')
        }
        var ch = stream.next();
        if (ch == '"' || ch == "'") {				// NOTE: ' or " or <<'SOMETHING'\n...\nSOMETHING\n or <<"SOMETHING"\n...\nSOMETHING\n
            if (stream.prefix(3) == "<<" + ch) {
                var p = stream.pos;
                stream.eatWhile(/\w/);
                var n = stream.current().substr(1);
                if (n && stream.eat(ch))
                    return tokenSOMETHING(stream, state, n);
                stream.pos = p
            }
            return tokenChain(stream, state, [ch], "string")
        }
        if (ch == "q") {
            var c = stream.look(-2);
            if (!(c && /\w/.test(c))) {
                c = stream.look(0);
                if (c == "x") {
                    c = stream.look(1);
                    if (c == "(") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, [")"], RXstyle, RXmodifiers)
                    }
                    if (c == "[") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, ["]"], RXstyle, RXmodifiers)
                    }
                    if (c == "{") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, ["}"], RXstyle, RXmodifiers)
                    }
                    if (c == "<") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, [">"], RXstyle, RXmodifiers)
                    }
                    if (/[\^'"!~\/]/.test(c)) {
                        stream.eatSuffix(1);
                        return tokenChain(stream, state, [stream.eat(c)], RXstyle, RXmodifiers)
                    }
                }
                else if (c == "q") {
                    c = stream.look(1);
                    if (c == "(") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, [")"], "string")
                    }
                    if (c == "[") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, ["]"], "string")
                    }
                    if (c == "{") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, ["}"], "string")
                    }
                    if (c == "<") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, [">"], "string")
                    }
                    if (/[\^'"!~\/]/.test(c)) {
                        stream.eatSuffix(1);
                        return tokenChain(stream, state, [stream.eat(c)], "string")
                    }
                }
                else if (c == "w") {
                    c = stream.look(1);
                    if (c == "(") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, [")"], "bracket")
                    }
                    if (c == "[") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, ["]"], "bracket")
                    }
                    if (c == "{") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, ["}"], "bracket")
                    }
                    if (c == "<") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, [">"], "bracket")
                    }
                    if (/[\^'"!~\/]/.test(c)) {
                        stream.eatSuffix(1);
                        return tokenChain(stream, state, [stream.eat(c)], "bracket")
                    }
                }
                else if (c == "r") {
                    c = stream.look(1);
                    if (c == "(") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, [")"], RXstyle, RXmodifiers)
                    }
                    if (c == "[") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, ["]"], RXstyle, RXmodifiers)
                    }
                    if (c == "{") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, ["}"], RXstyle, RXmodifiers)
                    }
                    if (c == "<") {
                        stream.eatSuffix(2);
                        return tokenChain(stream, state, [">"], RXstyle, RXmodifiers)
                    }
                    if (/[\^'"!~\/]/.test(c)) {
                        stream.eatSuffix(1);
                        return tokenChain(stream, state, [stream.eat(c)], RXstyle, RXmodifiers)
                    }
                }
                else if (/[\^'"!~\/(\[{<]/.test(c)) {
                    if (c == "(") {
                        stream.eatSuffix(1);
                        return tokenChain(stream, state, [")"], "string")
                    }
                    if (c == "[") {
                        stream.eatSuffix(1);
                        return tokenChain(stream, state, ["]"], "string")
                    }
                    if (c == "{") {
                        stream.eatSuffix(1);
                        return tokenChain(stream, state, ["}"], "string")
                    }
                    if (c == "<") {
                        stream.eatSuffix(1);
                        return tokenChain(stream, state, [">"], "string")
                    }
                    if (/[\^'"!~\/]/.test(c)) {
                        return tokenChain(stream, state, [stream.eat(c)], "string")
                    }
                }
            }
        }
        if (ch == "m") {
            var c = stream.look(-2);
            if (!(c && /\w/.test(c))) {
                c = stream.eat(/[(\[{<\^'"!~\/]/);
                if (c) {
                    if (/[\^'"!~\/]/.test(c)) {
                        return tokenChain(stream, state, [c], RXstyle, RXmodifiers)
                    }
                    if (c == "(") {
                        return tokenChain(stream, state, [")"], RXstyle, RXmodifiers)
                    }
                    if (c == "[") {
                        return tokenChain(stream, state, ["]"], RXstyle, RXmodifiers)
                    }
                    if (c == "{") {
                        return tokenChain(stream, state, ["}"], RXstyle, RXmodifiers)
                    }
                    if (c == "<") {
                        return tokenChain(stream, state, [">"], RXstyle, RXmodifiers)
                    }
                }
            }
        }
        if (ch == "s") {
            var c = /[\/>\]})\w]/.test(stream.look(-2));
            if (!c) {
                c = stream.eat(/[(\[{<\^'"!~\/]/);
                if (c) {
                    if (c == "[")
                        return tokenChain(stream, state, ["]", "]"], RXstyle, RXmodifiers);
                    if (c == "{")
                        return tokenChain(stream, state, ["}", "}"], RXstyle, RXmodifiers);
                    if (c == "<")
                        return tokenChain(stream, state, [">", ">"], RXstyle, RXmodifiers);
                    if (c == "(")
                        return tokenChain(stream, state, [")", ")"], RXstyle, RXmodifiers);
                    return tokenChain(stream, state, [c, c], RXstyle, RXmodifiers)
                }
            }
        }
        if (ch == "y") {
            var c = /[\/>\]})\w]/.test(stream.look(-2));
            if (!c) {
                c = stream.eat(/[(\[{<\^'"!~\/]/);
                if (c) {
                    if (c == "[")
                        return tokenChain(stream, state, ["]", "]"], RXstyle, RXmodifiers);
                    if (c == "{")
                        return tokenChain(stream, state, ["}", "}"], RXstyle, RXmodifiers);
                    if (c == "<")
                        return tokenChain(stream, state, [">", ">"], RXstyle, RXmodifiers);
                    if (c == "(")
                        return tokenChain(stream, state, [")", ")"], RXstyle, RXmodifiers);
                    return tokenChain(stream, state, [c, c], RXstyle, RXmodifiers)
                }
            }
        }
        if (ch == "t") {
            var c = /[\/>\]})\w]/.test(stream.look(-2));
            if (!c) {
                c = stream.eat("r"); if (c) {
                    c = stream.eat(/[(\[{<\^'"!~\/]/);
                    if (c) {
                        if (c == "[")
                            return tokenChain(stream, state, ["]", "]"], RXstyle, RXmodifiers);
                        if (c == "{")
                            return tokenChain(stream, state, ["}", "}"], RXstyle, RXmodifiers);
                        if (c == "<")
                            return tokenChain(stream, state, [">", ">"], RXstyle, RXmodifiers);
                        if (c == "(")
                            return tokenChain(stream, state, [")", ")"], RXstyle, RXmodifiers);
                        return tokenChain(stream, state, [c, c], RXstyle, RXmodifiers)
                    }
                }
            }
        }
        if (ch == "`") {
            return tokenChain(stream, state, [ch], "variable-2")
        }
        if (ch == "/") {
            return tokenChain(stream, state, [ch], RXstyle, RXmodifiers)
        }
        if (ch == "$") {
            var p = stream.pos;
            if (stream.eatWhile(/\d/) || stream.eat("{") && stream.eatWhile(/\d/) && stream.eat("}"))
                return "variable-2";
            else
                stream.pos = p
        }
        if (/[$@%]/.test(ch)) {
            var p = stream.pos;
            if (stream.eat("^") && stream.eat(/[A-Z]/) || !/[@$%&]/.test(stream.look(-2)) && stream.eat(/[=|\\\-#?@;:&`~\^!\[\]*'"$+.,\/<>()]/)) {
                var c = stream.current();
                if (PERL[c])
                    return "variable-2"
            }
            stream.pos = p
        }
        if (/[$@%&]/.test(ch)) {
            if (stream.eatWhile(/[\w$\[\]]/) || stream.eat("{") && stream.eatWhile(/[\w$\[\]]/) && stream.eat("}")) {
                var c = stream.current();
                if (PERL[c])
                    return "variable-2";
                else
                    return "variable"
            }
        }
        if (ch == "#") {
            if (stream.look(-2) != "$") {
                stream.skipToEnd();
                return "comment"
            }
        }
        if (/[:+\-\^*$&%@=<>!?|\/~\.]/.test(ch)) {
            var p = stream.pos;
            stream.eatWhile(/[:+\-\^*$&%@=<>!?|\/~\.]/);
            if (PERL[stream.current()])
                return "operator";
            else
                stream.pos = p
        }
        if (ch == "_") {
            if (stream.pos == 1) {
                if (stream.suffix(6) == "_END__") {
                    return tokenChain(stream, state, ['\0'], "comment")
                }
                else if (stream.suffix(7) == "_DATA__") {
                    return tokenChain(stream, state, ['\0'], "variable-2")
                }
                else if (stream.suffix(7) == "_C__") {
                    return tokenChain(stream, state, ['\0'], "string")
                }
            }
        }
        if (/\w/.test(ch)) {
            var p = stream.pos;
            if (stream.look(-2) == "{" && (stream.look(0) == "}" || stream.eatWhile(/\w/) && stream.look(0) == "}"))
                return "string";
            else
                stream.pos = p
        }
        if (/[A-Z]/.test(ch)) {
            var l = stream.look(-2);
            var p = stream.pos;
            stream.eatWhile(/[A-Z_]/);
            if (/[\da-z]/.test(stream.look(0))) {
                stream.pos = p
            }
            else {
                var c = PERL[stream.current()];
                if (!c)
                    return "meta";
                if (c[1])
                    c = c[0];
                if (l != ":") {
                    if (c == 1)
                        return "keyword";
                    else if (c == 2)
                        return "def";
                    else if (c == 3)
                        return "atom";
                    else if (c == 4)
                        return "operator";
                    else if (c == 5)
                        return "variable-2";
                    else
                        return "meta"
                }
                else
                    return "meta"
            }
        }
        if (/[a-zA-Z_]/.test(ch)) {
            var l = stream.look(-2);
            stream.eatWhile(/\w/);
            var c = PERL[stream.current()];
            if (!c)
                return "meta";
            if (c[1])
                c = c[0];
            if (l != ":") {
                if (c == 1)
                    return "keyword";
                else if (c == 2)
                    return "def";
                else if (c == 3)
                    return "atom";
                else if (c == 4)
                    return "operator";
                else if (c == 5)
                    return "variable-2";
                else
                    return "meta"
            }
            else
                return "meta"
        }
        return null
    }

    return {
        startState: function () {
            return {
                tokenize: tokenPerl,
                chain: null,
                style: null,
                tail: null
            }
        },
        token: function (stream, state) {
            return (state.tokenize || tokenPerl)(stream, state)
        },
        electricChars: "{}"
    }
});

CodeMirror.defineMIME("text/x-perl", "perl");

// it's like "peek", but need for look-ahead or look-behind if index < 0
CodeMirror.StringStream.prototype.look = function (c) {
    return this.string.charAt(this.pos + (c || 0))
};

// return a part of prefix of current stream from current position
CodeMirror.StringStream.prototype.prefix = function (c) {
    var x = this.pos - c;
    return this.string.substr((x >= 0 ? x : 0), c)
};

// return a part of suffix of current stream from current position
CodeMirror.StringStream.prototype.suffix = function (c) {
    var y = this.string.length;
    var x = y - this.pos + 1;
    return this.string.substr(this.pos, (c && c < y ? c : x))
};

// return a part of suffix of current stream from current position and change current position
CodeMirror.StringStream.prototype.nsuffix = function (c) {
    var p = this.pos;
    var l = c || (this.string.length - this.pos + 1);
    this.pos += l;
    return this.string.substr(p, l)
};

// eating and vomiting a part of stream from current position
CodeMirror.StringStream.prototype.eatSuffix = function (c) {
    var x = this.pos + c;
    var y;
    if (x <= 0)
        this.pos = 0;
    else if (x >= (y = this.string.length - 1))
        this.pos = y;
    else
        this.pos = x
};
CodeMirror.defineMode("pascal", function (config) {
    function words(str) {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
        return obj;
    }
    var keywords = words("and array begin case const div do downto else end file for forward integer " +
                         "boolean char function goto if in label mod nil not of or packed procedure " +
                         "program record repeat set string then to type until var while with");
    var blockKeywords = words("case do else for if switch while struct then of");
    var atoms = { "null": true };

    var isOperatorChar = /[+\-*&%=<>!?|\/]/;
    var curPunc;

    function tokenBase(stream, state) {
        var ch = stream.next();
        if (ch == "#" && state.startOfLine) {
            stream.skipToEnd();
            return "meta";
        }
        if (ch == '"' || ch == "'") {
            state.tokenize = tokenString(ch);
            return state.tokenize(stream, state);
        }
        if (ch == "(" && stream.eat("*")) {
            state.tokenize = tokenComment;
            return tokenComment(stream, state);
        }
        if (/[\[\]{}\(\),;\:\.]/.test(ch)) {
            curPunc = ch;
            return null
        }
        if (/\d/.test(ch)) {
            stream.eatWhile(/[\w\.]/);
            return "number";
        }
        if (ch == "/") {
            if (stream.eat("/")) {
                stream.skipToEnd();
                return "comment";
            }
        }
        if (isOperatorChar.test(ch)) {
            stream.eatWhile(isOperatorChar);
            return "operator";
        }
        stream.eatWhile(/[\w\$_]/);
        var cur = stream.current();
        if (keywords.propertyIsEnumerable(cur)) {
            if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
            return "keyword";
        }
        if (atoms.propertyIsEnumerable(cur)) return "atom";
        return "word";
    }

    function tokenString(quote) {
        return function (stream, state) {
            var escaped = false, next, end = false;
            while ((next = stream.next()) != null) {
                if (next == quote && !escaped) { end = true; break; }
                escaped = !escaped && next == "\\";
            }
            if (end || !escaped) state.tokenize = null;
            return "string";
        };
    }

    function tokenComment(stream, state) {
        var maybeEnd = false, ch;
        while (ch = stream.next()) {
            if (ch == ")" && maybeEnd) {
                state.tokenize = null;
                break;
            }
            maybeEnd = (ch == "*");
        }
        return "comment";
    }

    function Context(indented, column, type, align, prev) {
        this.indented = indented;
        this.column = column;
        this.type = type;
        this.align = align;
        this.prev = prev;
    }
    function pushContext(state, col, type) {
        return state.context = new Context(state.indented, col, type, null, state.context);
    }
    function popContext(state) {
        var t = state.context.type;
        if (t == ")" || t == "]")
            state.indented = state.context.indented;
        return state.context = state.context.prev;
    }

    // Interface

    return {
        startState: function (basecolumn) {
            return {
                tokenize: null,
                context: new Context((basecolumn || 0) - config.indentUnit, 0, "top", false),
                indented: 0,
                startOfLine: true
            };
        },

        token: function (stream, state) {
            var ctx = state.context;
            if (stream.sol()) {
                if (ctx.align == null) ctx.align = false;
                state.indented = stream.indentation();
                state.startOfLine = true;
            }
            if (stream.eatSpace()) return null;
            curPunc = null;
            var style = (state.tokenize || tokenBase)(stream, state);
            if (style == "comment" || style == "meta") return style;
            if (ctx.align == null) ctx.align = true;

            if ((curPunc == ";" || curPunc == ":") && ctx.type == "statement") popContext(state);
            else if (curPunc == "[") pushContext(state, stream.column(), "]");
            else if (curPunc == "(") pushContext(state, stream.column(), ")");
            else if (curPunc == ctx.type) popContext(state);
            else if (ctx.type == "top" || (ctx.type == "statement" && curPunc == "newstatement"))
                pushContext(state, stream.column(), "statement");
            state.startOfLine = false;
            return style;
        },

        electricChars: "{}"
    };
});

CodeMirror.defineMIME("text/x-pascal", "pascal");
/**********************************************************
* This script provides syntax highlighting support for 
* the Ntriples format.
* Ntriples format specification: 
*     http://www.w3.org/TR/rdf-testcases/#ntriples
***********************************************************/

/* 
    The following expression defines the defined ASF grammar transitions.

    pre_subject ->
        {
        ( writing_subject_uri | writing_bnode_uri )
            -> pre_predicate 
                -> writing_predicate_uri 
                    -> pre_object 
                        -> writing_object_uri | writing_object_bnode | 
                          ( 
                            writing_object_literal 
                                -> writing_literal_lang | writing_literal_type
                          )
                            -> post_object
                                -> BEGIN
         } otherwise {
             -> ERROR
         }
*/
CodeMirror.defineMode("ntriples", function () {

    Location = {
        PRE_SUBJECT: 0,
        WRITING_SUB_URI: 1,
        WRITING_BNODE_URI: 2,
        PRE_PRED: 3,
        WRITING_PRED_URI: 4,
        PRE_OBJ: 5,
        WRITING_OBJ_URI: 6,
        WRITING_OBJ_BNODE: 7,
        WRITING_OBJ_LITERAL: 8,
        WRITING_LIT_LANG: 9,
        WRITING_LIT_TYPE: 10,
        POST_OBJ: 11,
        ERROR: 12
    };
    function transitState(currState, c) {
        var currLocation = currState.location;
        var ret;

        // Opening.
        if (currLocation == Location.PRE_SUBJECT && c == '<') ret = Location.WRITING_SUB_URI;
        else if (currLocation == Location.PRE_SUBJECT && c == '_') ret = Location.WRITING_BNODE_URI;
        else if (currLocation == Location.PRE_PRED && c == '<') ret = Location.WRITING_PRED_URI;
        else if (currLocation == Location.PRE_OBJ && c == '<') ret = Location.WRITING_OBJ_URI;
        else if (currLocation == Location.PRE_OBJ && c == '_') ret = Location.WRITING_OBJ_BNODE;
        else if (currLocation == Location.PRE_OBJ && c == '"') ret = Location.WRITING_OBJ_LITERAL;

            // Closing.
        else if (currLocation == Location.WRITING_SUB_URI && c == '>') ret = Location.PRE_PRED;
        else if (currLocation == Location.WRITING_BNODE_URI && c == ' ') ret = Location.PRE_PRED;
        else if (currLocation == Location.WRITING_PRED_URI && c == '>') ret = Location.PRE_OBJ;
        else if (currLocation == Location.WRITING_OBJ_URI && c == '>') ret = Location.POST_OBJ;
        else if (currLocation == Location.WRITING_OBJ_BNODE && c == ' ') ret = Location.POST_OBJ;
        else if (currLocation == Location.WRITING_OBJ_LITERAL && c == '"') ret = Location.POST_OBJ;
        else if (currLocation == Location.WRITING_LIT_LANG && c == ' ') ret = Location.POST_OBJ;
        else if (currLocation == Location.WRITING_LIT_TYPE && c == '>') ret = Location.POST_OBJ;

            // Closing typed and language literal.
        else if (currLocation == Location.WRITING_OBJ_LITERAL && c == '@') ret = Location.WRITING_LIT_LANG;
        else if (currLocation == Location.WRITING_OBJ_LITERAL && c == '^') ret = Location.WRITING_LIT_TYPE;

            // Spaces.
        else if (c == ' ' &&
                 (
                   currLocation == Location.PRE_SUBJECT ||
                   currLocation == Location.PRE_PRED ||
                   currLocation == Location.PRE_OBJ ||
                   currLocation == Location.POST_OBJ
                 )
               ) ret = currLocation;

            // Reset.
        else if (currLocation == Location.POST_OBJ && c == '.') ret = Location.PRE_SUBJECT;

            // Error
        else ret = Location.ERROR;

        currState.location = ret;
    }

    untilSpace = function (c) { return c != ' '; };
    untilEndURI = function (c) { return c != '>'; };
    return {
        startState: function () {
            return {
                location: Location.PRE_SUBJECT,
                uris: [],
                anchors: [],
                bnodes: [],
                langs: [],
                types: []
            };
        },
        token: function (stream, state) {
            var ch = stream.next();
            if (ch == '<') {
                transitState(state, ch);
                var parsedURI = '';
                stream.eatWhile(function (c) { if (c != '#' && c != '>') { parsedURI += c; return true; } return false; });
                state.uris.push(parsedURI);
                if (stream.match('#', false)) return 'variable';
                stream.next();
                transitState(state, '>');
                return 'variable';
            }
            if (ch == '#') {
                var parsedAnchor = '';
                stream.eatWhile(function (c) { if (c != '>' && c != ' ') { parsedAnchor += c; return true; } return false });
                state.anchors.push(parsedAnchor);
                return 'variable-2';
            }
            if (ch == '>') {
                transitState(state, '>');
                return 'variable';
            }
            if (ch == '_') {
                transitState(state, ch);
                var parsedBNode = '';
                stream.eatWhile(function (c) { if (c != ' ') { parsedBNode += c; return true; } return false; });
                state.bnodes.push(parsedBNode);
                stream.next();
                transitState(state, ' ');
                return 'builtin';
            }
            if (ch == '"') {
                transitState(state, ch);
                stream.eatWhile(function (c) { return c != '"'; });
                stream.next();
                if (stream.peek() != '@' && stream.peek() != '^') {
                    transitState(state, '"');
                }
                return 'string';
            }
            if (ch == '@') {
                transitState(state, '@');
                var parsedLang = '';
                stream.eatWhile(function (c) { if (c != ' ') { parsedLang += c; return true; } return false; });
                state.langs.push(parsedLang);
                stream.next();
                transitState(state, ' ');
                return 'string-2';
            }
            if (ch == '^') {
                stream.next();
                transitState(state, '^');
                var parsedType = '';
                stream.eatWhile(function (c) { if (c != '>') { parsedType += c; return true; } return false; });
                state.types.push(parsedType);
                stream.next();
                transitState(state, '>');
                return 'variable';
            }
            if (ch == ' ') {
                transitState(state, ch);
            }
            if (ch == '.') {
                transitState(state, ch);
            }
        }
    };
});

CodeMirror.defineMIME("text/n-triples", "ntriples");
CodeMirror.defineMode("markdown", function (cmCfg, modeCfg) {

    var htmlMode = CodeMirror.getMode(cmCfg, { name: 'xml', htmlMode: true });

    var header = 'header'
    , code = 'code'
    , quote = 'quote'
    , list = 'list'
    , hr = 'hr'
    , linktext = 'linktext'
    , linkhref = 'linkhref'
    , em = 'em'
    , strong = 'strong'
    , emstrong = 'emstrong';

    var hrRE = /^[*-=_]/
    , ulRE = /^[*-+]\s+/
    , olRE = /^[0-9]\.\s+/
    , headerRE = /^(?:\={3,}|-{3,})$/
    , codeRE = /^(k:\t|\s{4,})/
    , textRE = /^[^\[*_\\<>`]+/;

    function switchInline(stream, state, f) {
        state.f = state.inline = f;
        return f(stream, state);
    }

    function switchBlock(stream, state, f) {
        state.f = state.block = f;
        return f(stream, state);
    }


    // Blocks

    function blockNormal(stream, state) {
        if (stream.match(codeRE)) {
            stream.skipToEnd();
            return code;
        }

        if (stream.eatSpace()) {
            return null;
        }

        if (stream.peek() === '#' || stream.match(headerRE)) {
            stream.skipToEnd();
            return header;
        }
        if (stream.eat('>')) {
            state.indentation++;
            return quote;
        }
        if (stream.peek() === '<') {
            return switchBlock(stream, state, htmlBlock);
        }
        if (stream.peek() === '[') {
            return switchInline(stream, state, footnoteLink);
        }
        if (hrRE.test(stream.peek())) {
            var re = new RegExp('(?:\s*[' + stream.peek() + ']){3,}$');
            if (stream.match(re, true)) {
                return hr;
            }
        }

        var match;
        if (match = stream.match(ulRE, true) || stream.match(olRE, true)) {
            state.indentation += match[0].length;
            return list;
        }

        return switchInline(stream, state, state.inline);
    }

    function htmlBlock(stream, state) {
        var type = htmlMode.token(stream, state.htmlState);
        if (stream.eol() && !state.htmlState.context) {
            state.block = blockNormal;
        }
        return type;
    }


    // Inline

    function inlineNormal(stream, state) {
        function getType() {
            return state.strong ? (state.em ? emstrong : strong)
                                : (state.em ? em : null);
        }

        if (stream.match(textRE, true)) {
            return getType();
        }

        var ch = stream.next();

        if (ch === '\\') {
            stream.next();
            return getType();
        }
        if (ch === '`') {
            return switchInline(stream, state, inlineElement(code, '`'));
        }
        if (ch === '<') {
            return switchInline(stream, state, inlineElement(linktext, '>'));
        }
        if (ch === '[') {
            return switchInline(stream, state, linkText);
        }

        var t = getType();
        if (ch === '*' || ch === '_') {
            if (stream.eat(ch)) {
                return (state.strong = !state.strong) ? getType() : t;
            }
            return (state.em = !state.em) ? getType() : t;
        }

        return getType();
    }

    function linkText(stream, state) {
        while (!stream.eol()) {
            var ch = stream.next();
            if (ch === '\\') stream.next();
            if (ch === ']') {
                state.inline = state.f = linkHref;
                return linktext;
            }
        }
        return linktext;
    }

    function linkHref(stream, state) {
        stream.eatSpace();
        var ch = stream.next();
        if (ch === '(' || ch === '[') {
            return switchInline(stream, state, inlineElement(linkhref, ch === '(' ? ')' : ']'));
        }
        return 'error';
    }

    function footnoteLink(stream, state) {
        if (stream.match(/^[^\]]*\]:/, true)) {
            state.f = footnoteUrl;
            return linktext;
        }
        return switchInline(stream, state, inlineNormal);
    }

    function footnoteUrl(stream, state) {
        stream.eatSpace();
        stream.match(/^[^\s]+/, true);
        state.f = state.inline = inlineNormal;
        return linkhref;
    }

    function inlineElement(type, endChar, next) {
        next = next || inlineNormal;
        return function (stream, state) {
            while (!stream.eol()) {
                var ch = stream.next();
                if (ch === '\\') stream.next();
                if (ch === endChar) {
                    state.inline = state.f = next;
                    return type;
                }
            }
            return type;
        };
    }

    return {
        startState: function () {
            return {
                f: blockNormal,

                block: blockNormal,
                htmlState: htmlMode.startState(),
                indentation: 0,

                inline: inlineNormal,
                em: false,
                strong: false
            };
        },

        copyState: function (s) {
            return {
                f: s.f,

                block: s.block,
                htmlState: CodeMirror.copyState(htmlMode, s.htmlState),
                indentation: s.indentation,

                inline: s.inline,
                em: s.em,
                strong: s.strong
            };
        },

        token: function (stream, state) {
            if (stream.sol()) {
                state.f = state.block;
                var previousIndentation = state.indentation
                , currentIndentation = 0;
                while (previousIndentation > 0) {
                    if (stream.eat(' ')) {
                        previousIndentation--;
                        currentIndentation++;
                    } else if (previousIndentation >= 4 && stream.eat('\t')) {
                        previousIndentation -= 4;
                        currentIndentation += 4;
                    } else {
                        break;
                    }
                }
                state.indentation = currentIndentation;

                if (currentIndentation > 0) return null;
            }
            return state.f(stream, state);
        }
    };

});

CodeMirror.defineMIME("text/x-markdown", "markdown");
// LUA mode. Ported to CodeMirror 2 from Franciszek Wawrzak's
// CodeMirror 1 mode.
// highlights keywords, strings, comments (no leveling supported! ("[==[")), tokens, basic indenting

CodeMirror.defineMode("lua", function (config, parserConfig) {
    var indentUnit = config.indentUnit;

    function prefixRE(words) {
        return new RegExp("^(?:" + words.join("|") + ")", "i");
    }
    function wordRE(words) {
        return new RegExp("^(?:" + words.join("|") + ")$", "i");
    }
    var specials = wordRE(parserConfig.specials || []);

    // long list of standard functions from lua manual
    var builtins = wordRE([
      "_G", "_VERSION", "assert", "collectgarbage", "dofile", "error", "getfenv", "getmetatable", "ipairs", "load",
      "loadfile", "loadstring", "module", "next", "pairs", "pcall", "print", "rawequal", "rawget", "rawset", "require",
      "select", "setfenv", "setmetatable", "tonumber", "tostring", "type", "unpack", "xpcall",

      "coroutine.create", "coroutine.resume", "coroutine.running", "coroutine.status", "coroutine.wrap", "coroutine.yield",

      "debug.debug", "debug.getfenv", "debug.gethook", "debug.getinfo", "debug.getlocal", "debug.getmetatable",
      "debug.getregistry", "debug.getupvalue", "debug.setfenv", "debug.sethook", "debug.setlocal", "debug.setmetatable",
      "debug.setupvalue", "debug.traceback",

      "close", "flush", "lines", "read", "seek", "setvbuf", "write",

      "io.close", "io.flush", "io.input", "io.lines", "io.open", "io.output", "io.popen", "io.read", "io.stderr", "io.stdin",
      "io.stdout", "io.tmpfile", "io.type", "io.write",

      "math.abs", "math.acos", "math.asin", "math.atan", "math.atan2", "math.ceil", "math.cos", "math.cosh", "math.deg",
      "math.exp", "math.floor", "math.fmod", "math.frexp", "math.huge", "math.ldexp", "math.log", "math.log10", "math.max",
      "math.min", "math.modf", "math.pi", "math.pow", "math.rad", "math.random", "math.randomseed", "math.sin", "math.sinh",
      "math.sqrt", "math.tan", "math.tanh",

      "os.clock", "os.date", "os.difftime", "os.execute", "os.exit", "os.getenv", "os.remove", "os.rename", "os.setlocale",
      "os.time", "os.tmpname",

      "package.cpath", "package.loaded", "package.loaders", "package.loadlib", "package.path", "package.preload",
      "package.seeall",

      "string.byte", "string.char", "string.dump", "string.find", "string.format", "string.gmatch", "string.gsub",
      "string.len", "string.lower", "string.match", "string.rep", "string.reverse", "string.sub", "string.upper",

      "table.concat", "table.insert", "table.maxn", "table.remove", "table.sort"
    ]);
    var keywords = wordRE(["and", "break", "elseif", "false", "nil", "not", "or", "return",
               "true", "function", "end", "if", "then", "else", "do",
               "while", "repeat", "until", "for", "in", "local"]);

    var indentTokens = wordRE(["function", "if", "repeat", "do", "\\(", "{"]);
    var dedentTokens = wordRE(["end", "until", "\\)", "}"]);
    var dedentPartial = prefixRE(["end", "until", "\\)", "}", "else", "elseif"]);

    function readBracket(stream) {
        var level = 0;
        while (stream.eat("="))++level;
        stream.eat("[");
        return level;
    }

    function normal(stream, state) {
        var ch = stream.next();
        if (ch == "-" && stream.eat("-")) {
            if (stream.eat("["))
                return (state.cur = bracketed(readBracket(stream), "comment"))(stream, state);
            stream.skipToEnd();
            return "comment";
        }
        if (ch == "\"" || ch == "'")
            return (state.cur = string(ch))(stream, state);
        if (ch == "[" && /[\[=]/.test(stream.peek()))
            return (state.cur = bracketed(readBracket(stream), "string"))(stream, state);
        if (/\d/.test(ch)) {
            stream.eatWhile(/[\w.%]/);
            return "number";
        }
        if (/[\w_]/.test(ch)) {
            stream.eatWhile(/[\w\\\-_.]/);
            return "variable";
        }
        return null;
    }

    function bracketed(level, style) {
        return function (stream, state) {
            var curlev = null, ch;
            while ((ch = stream.next()) != null) {
                if (curlev == null) { if (ch == "]") curlev = 0; }
                else if (ch == "=")++curlev;
                else if (ch == "]" && curlev == level) { state.cur = normal; break; }
                else curlev = null;
            }
            return style;
        };
    }

    function string(quote) {
        return function (stream, state) {
            var escaped = false, ch;
            while ((ch = stream.next()) != null) {
                if (ch == quote && !escaped) break;
                escaped = !escaped && ch == "\\";
            }
            if (!escaped) state.cur = normal;
            return "string";
        };
    }

    return {
        startState: function (basecol) {
            return { basecol: basecol || 0, indentDepth: 0, cur: normal };
        },

        token: function (stream, state) {
            if (stream.eatSpace()) return null;
            var style = state.cur(stream, state);
            var word = stream.current();
            if (style == "variable") {
                if (keywords.test(word)) style = "keyword";
                else if (builtins.test(word)) style = "builtin";
                else if (specials.test(word)) style = "variable-2";
            }
            if ((style != "lua-comment") && (style != "lua-string")) {
                if (indentTokens.test(word))++state.indentDepth;
                else if (dedentTokens.test(word))--state.indentDepth;
            }
            return style;
        },

        indent: function (state, textAfter) {
            var closing = dedentPartial.test(textAfter);
            return state.basecol + indentUnit * (state.indentDepth - (closing ? 1 : 0));
        }
    };
});

CodeMirror.defineMIME("text/x-lua", "lua");
CodeMirror.defineMode("jinja2", function (config, parserConf) {
    var keywords = ["block", "endblock", "for", "endfor", "in", "true", "false",
                    "loop", "none", "self", "super", "if", "as", "not", "and",
                    "else", "import", "with", "without", "context"];
    keywords = new RegExp("^((" + keywords.join(")|(") + "))\\b");

    function tokenBase(stream, state) {
        var ch = stream.next();
        if (ch == "{") {
            if (ch = stream.eat(/\{|%|#/)) {
                stream.eat("-");
                state.tokenize = inTag(ch);
                return "tag";
            }
        }
    }
    function inTag(close) {
        if (close == "{") {
            close = "}";
        }
        return function (stream, state) {
            var ch = stream.next();
            if ((ch == close || (ch == "-" && stream.eat(close)))
                && stream.eat("}")) {
                state.tokenize = tokenBase;
                return "tag";
            }
            if (stream.match(keywords)) {
                return "keyword";
            }
            return close == "#" ? "comment" : "string";
        };
    }
    return {
        startState: function () {
            return { tokenize: tokenBase };
        },
        token: function (stream, state) {
            return state.tokenize(stream, state);
        }
    };
});

CodeMirror.defineMode("javascript", function (config, parserConfig) {
    var indentUnit = config.indentUnit;
    var jsonMode = parserConfig.json;

    // Tokenizer

    var keywords = function () {
        function kw(type) { return { type: type, style: "keyword" }; }
        var A = kw("keyword a"), B = kw("keyword b"), C = kw("keyword c");
        var operator = kw("operator"), atom = { type: "atom", style: "atom" };
        return {
            "if": A, "while": A, "with": A, "else": B, "do": B, "try": B, "finally": B,
            "return": C, "break": C, "continue": C, "new": C, "delete": C, "throw": C,
            "var": kw("var"), "function": kw("function"), "catch": kw("catch"),
            "for": kw("for"), "switch": kw("switch"), "case": kw("case"), "default": kw("default"),
            "in": operator, "typeof": operator, "instanceof": operator,
            "true": atom, "false": atom, "null": atom, "undefined": atom, "NaN": atom, "Infinity": atom
        };
    }();

    var isOperatorChar = /[+\-*&%=<>!?|]/;

    function chain(stream, state, f) {
        state.tokenize = f;
        return f(stream, state);
    }

    function nextUntilUnescaped(stream, end) {
        var escaped = false, next;
        while ((next = stream.next()) != null) {
            if (next == end && !escaped)
                return false;
            escaped = !escaped && next == "\\";
        }
        return escaped;
    }

    // Used as scratch variables to communicate multiple values without
    // consing up tons of objects.
    var type, content;
    function ret(tp, style, cont) {
        type = tp; content = cont;
        return style;
    }

    function jsTokenBase(stream, state) {
        var ch = stream.next();
        if (ch == '"' || ch == "'")
            return chain(stream, state, jsTokenString(ch));
        else if (/[\[\]{}\(\),;\:\.]/.test(ch))
            return ret(ch);
        else if (ch == "0" && stream.eat(/x/i)) {
            stream.eatWhile(/[\da-f]/i);
            return ret("number", "number");
        }
        else if (/\d/.test(ch)) {
            stream.match(/^\d*(?:\.\d*)?(?:[eE][+\-]?\d+)?/);
            return ret("number", "number");
        }
        else if (ch == "/") {
            if (stream.eat("*")) {
                return chain(stream, state, jsTokenComment);
            }
            else if (stream.eat("/")) {
                stream.skipToEnd();
                return ret("comment", "comment");
            }
            else if (state.reAllowed) {
                nextUntilUnescaped(stream, "/");
                stream.eatWhile(/[gimy]/); // 'y' is "sticky" option in Mozilla
                return ret("regexp", "string");
            }
            else {
                stream.eatWhile(isOperatorChar);
                return ret("operator", null, stream.current());
            }
        }
        else if (ch == "#") {
            stream.skipToEnd();
            return ret("error", "error");
        }
        else if (isOperatorChar.test(ch)) {
            stream.eatWhile(isOperatorChar);
            return ret("operator", null, stream.current());
        }
        else {
            stream.eatWhile(/[\w\$_]/);
            var word = stream.current(), known = keywords.propertyIsEnumerable(word) && keywords[word];
            return known ? ret(known.type, known.style, word) :
                           ret("variable", "variable", word);
        }
    }

    function jsTokenString(quote) {
        return function (stream, state) {
            if (!nextUntilUnescaped(stream, quote))
                state.tokenize = jsTokenBase;
            return ret("string", "string");
        };
    }

    function jsTokenComment(stream, state) {
        var maybeEnd = false, ch;
        while (ch = stream.next()) {
            if (ch == "/" && maybeEnd) {
                state.tokenize = jsTokenBase;
                break;
            }
            maybeEnd = (ch == "*");
        }
        return ret("comment", "comment");
    }

    // Parser

    var atomicTypes = { "atom": true, "number": true, "variable": true, "string": true, "regexp": true };

    function JSLexical(indented, column, type, align, prev, info) {
        this.indented = indented;
        this.column = column;
        this.type = type;
        this.prev = prev;
        this.info = info;
        if (align != null) this.align = align;
    }

    function inScope(state, varname) {
        for (var v = state.localVars; v; v = v.next)
            if (v.name == varname) return true;
    }

    function parseJS(state, style, type, content, stream) {
        var cc = state.cc;
        // Communicate our context to the combinators.
        // (Less wasteful than consing up a hundred closures on every call.)
        cx.state = state; cx.stream = stream; cx.marked = null, cx.cc = cc;

        if (!state.lexical.hasOwnProperty("align"))
            state.lexical.align = true;

        while (true) {
            var combinator = cc.length ? cc.pop() : jsonMode ? expression : statement;
            if (combinator(type, content)) {
                while (cc.length && cc[cc.length - 1].lex)
                    cc.pop()();
                if (cx.marked) return cx.marked;
                if (type == "variable" && inScope(state, content)) return "variable-2";
                return style;
            }
        }
    }

    // Combinator utils

    var cx = { state: null, column: null, marked: null, cc: null };
    function pass() {
        for (var i = arguments.length - 1; i >= 0; i--) cx.cc.push(arguments[i]);
    }
    function cont() {
        pass.apply(null, arguments);
        return true;
    }
    function register(varname) {
        var state = cx.state;
        if (state.context) {
            cx.marked = "def";
            for (var v = state.localVars; v; v = v.next)
                if (v.name == varname) return;
            state.localVars = { name: varname, next: state.localVars };
        }
    }

    // Combinators

    var defaultVars = { name: "this", next: { name: "arguments" } };
    function pushcontext() {
        if (!cx.state.context) cx.state.localVars = defaultVars;
        cx.state.context = { prev: cx.state.context, vars: cx.state.localVars };
    }
    function popcontext() {
        cx.state.localVars = cx.state.context.vars;
        cx.state.context = cx.state.context.prev;
    }
    function pushlex(type, info) {
        var result = function () {
            var state = cx.state;
            state.lexical = new JSLexical(state.indented, cx.stream.column(), type, null, state.lexical, info)
        };
        result.lex = true;
        return result;
    }
    function poplex() {
        var state = cx.state;
        if (state.lexical.prev) {
            if (state.lexical.type == ")")
                state.indented = state.lexical.indented;
            state.lexical = state.lexical.prev;
        }
    }
    poplex.lex = true;

    function expect(wanted) {
        return function expecting(type) {
            if (type == wanted) return cont();
            else if (wanted == ";") return pass();
            else return cont(arguments.callee);
        };
    }

    function statement(type) {
        if (type == "var") return cont(pushlex("vardef"), vardef1, expect(";"), poplex);
        if (type == "keyword a") return cont(pushlex("form"), expression, statement, poplex);
        if (type == "keyword b") return cont(pushlex("form"), statement, poplex);
        if (type == "{") return cont(pushlex("}"), block, poplex);
        if (type == ";") return cont();
        if (type == "function") return cont(functiondef);
        if (type == "for") return cont(pushlex("form"), expect("("), pushlex(")"), forspec1, expect(")"),
                                          poplex, statement, poplex);
        if (type == "variable") return cont(pushlex("stat"), maybelabel);
        if (type == "switch") return cont(pushlex("form"), expression, pushlex("}", "switch"), expect("{"),
                                             block, poplex, poplex);
        if (type == "case") return cont(expression, expect(":"));
        if (type == "default") return cont(expect(":"));
        if (type == "catch") return cont(pushlex("form"), pushcontext, expect("("), funarg, expect(")"),
                                            statement, poplex, popcontext);
        return pass(pushlex("stat"), expression, expect(";"), poplex);
    }
    function expression(type) {
        if (atomicTypes.hasOwnProperty(type)) return cont(maybeoperator);
        if (type == "function") return cont(functiondef);
        if (type == "keyword c") return cont(expression);
        if (type == "(") return cont(pushlex(")"), expression, expect(")"), poplex, maybeoperator);
        if (type == "operator") return cont(expression);
        if (type == "[") return cont(pushlex("]"), commasep(expression, "]"), poplex, maybeoperator);
        if (type == "{") return cont(pushlex("}"), commasep(objprop, "}"), poplex, maybeoperator);
        return cont();
    }
    function maybeoperator(type, value) {
        if (type == "operator" && /\+\+|--/.test(value)) return cont(maybeoperator);
        if (type == "operator") return cont(expression);
        if (type == ";") return;
        if (type == "(") return cont(pushlex(")"), commasep(expression, ")"), poplex, maybeoperator);
        if (type == ".") return cont(property, maybeoperator);
        if (type == "[") return cont(pushlex("]"), expression, expect("]"), poplex, maybeoperator);
    }
    function maybelabel(type) {
        if (type == ":") return cont(poplex, statement);
        return pass(maybeoperator, expect(";"), poplex);
    }
    function property(type) {
        if (type == "variable") { cx.marked = "property"; return cont(); }
    }
    function objprop(type) {
        if (type == "variable") cx.marked = "property";
        if (atomicTypes.hasOwnProperty(type)) return cont(expect(":"), expression);
    }
    function commasep(what, end) {
        function proceed(type) {
            if (type == ",") return cont(what, proceed);
            if (type == end) return cont();
            return cont(expect(end));
        }
        return function commaSeparated(type) {
            if (type == end) return cont();
            else return pass(what, proceed);
        };
    }
    function block(type) {
        if (type == "}") return cont();
        return pass(statement, block);
    }
    function vardef1(type, value) {
        if (type == "variable") { register(value); return cont(vardef2); }
        return cont();
    }
    function vardef2(type, value) {
        if (value == "=") return cont(expression, vardef2);
        if (type == ",") return cont(vardef1);
    }
    function forspec1(type) {
        if (type == "var") return cont(vardef1, forspec2);
        if (type == ";") return pass(forspec2);
        if (type == "variable") return cont(formaybein);
        return pass(forspec2);
    }
    function formaybein(type, value) {
        if (value == "in") return cont(expression);
        return cont(maybeoperator, forspec2);
    }
    function forspec2(type, value) {
        if (type == ";") return cont(forspec3);
        if (value == "in") return cont(expression);
        return cont(expression, expect(";"), forspec3);
    }
    function forspec3(type) {
        if (type != ")") cont(expression);
    }
    function functiondef(type, value) {
        if (type == "variable") { register(value); return cont(functiondef); }
        if (type == "(") return cont(pushlex(")"), pushcontext, commasep(funarg, ")"), poplex, statement, popcontext);
    }
    function funarg(type, value) {
        if (type == "variable") { register(value); return cont(); }
    }

    // Interface

    return {
        startState: function (basecolumn) {
            return {
                tokenize: jsTokenBase,
                reAllowed: true,
                cc: [],
                lexical: new JSLexical((basecolumn || 0) - indentUnit, 0, "block", false),
                localVars: null,
                context: null,
                indented: 0
            };
        },

        token: function (stream, state) {
            if (stream.sol()) {
                if (!state.lexical.hasOwnProperty("align"))
                    state.lexical.align = false;
                state.indented = stream.indentation();
            }
            if (stream.eatSpace()) return null;
            var style = state.tokenize(stream, state);
            if (type == "comment") return style;
            state.reAllowed = type == "operator" || type == "keyword c" || type.match(/^[\[{}\(,;:]$/);
            return parseJS(state, style, type, content, stream);
        },

        indent: function (state, textAfter) {
            if (state.tokenize != jsTokenBase) return 0;
            var firstChar = textAfter && textAfter.charAt(0), lexical = state.lexical,
                type = lexical.type, closing = firstChar == type;
            if (type == "vardef") return lexical.indented + 4;
            else if (type == "form" && firstChar == "{") return lexical.indented;
            else if (type == "stat" || type == "form") return lexical.indented + indentUnit;
            else if (lexical.info == "switch" && !closing)
                return lexical.indented + (/^(?:case|default)\b/.test(textAfter) ? indentUnit : 2 * indentUnit);
            else if (lexical.align) return lexical.column + (closing ? 0 : 1);
            else return lexical.indented + (closing ? 0 : indentUnit);
        },

        electricChars: ":{}"
    };
});

CodeMirror.defineMIME("text/javascript", "javascript");
CodeMirror.defineMIME("application/json", { name: "javascript", json: true });

CodeMirror.defineMode("htmlmixed", function (config, parserConfig) {
    var htmlMode = CodeMirror.getMode(config, { name: "xml", htmlMode: true });
    var jsMode = CodeMirror.getMode(config, "javascript");
    var cssMode = CodeMirror.getMode(config, "css");

    function html(stream, state) {
        var style = htmlMode.token(stream, state.htmlState);
        if (style == "tag" && stream.current() == ">" && state.htmlState.context) {
            if (/^script$/i.test(state.htmlState.context.tagName)) {
                state.token = javascript;
                state.localState = jsMode.startState(htmlMode.indent(state.htmlState, ""));
                state.mode = "javascript";
            }
            else if (/^style$/i.test(state.htmlState.context.tagName)) {
                state.token = css;
                state.localState = cssMode.startState(htmlMode.indent(state.htmlState, ""));
                state.mode = "css";
            }
        }
        return style;
    }
    function maybeBackup(stream, pat, style) {
        var cur = stream.current();
        var close = cur.search(pat);
        if (close > -1) stream.backUp(cur.length - close);
        return style;
    }
    function javascript(stream, state) {
        if (stream.match(/^<\/\s*script\s*>/i, false)) {
            state.token = html;
            state.curState = null;
            state.mode = "html";
            return html(stream, state);
        }
        return maybeBackup(stream, /<\/\s*script\s*>/,
                           jsMode.token(stream, state.localState));
    }
    function css(stream, state) {
        if (stream.match(/^<\/\s*style\s*>/i, false)) {
            state.token = html;
            state.localState = null;
            state.mode = "html";
            return html(stream, state);
        }
        return maybeBackup(stream, /<\/\s*style\s*>/,
                           cssMode.token(stream, state.localState));
    }

    return {
        startState: function () {
            var state = htmlMode.startState();
            return { token: html, localState: null, mode: "html", htmlState: state };
        },

        copyState: function (state) {
            if (state.localState)
                var local = CodeMirror.copyState(state.token == css ? cssMode : jsMode, state.localState);
            return {
                token: state.token, localState: local, mode: state.mode,
                htmlState: CodeMirror.copyState(htmlMode, state.htmlState)
            };
        },

        token: function (stream, state) {
            return state.token(stream, state);
        },

        indent: function (state, textAfter) {
            if (state.token == html || /^\s*<\//.test(textAfter))
                return htmlMode.indent(state.htmlState, textAfter);
            else if (state.token == javascript)
                return jsMode.indent(state.localState, textAfter);
            else
                return cssMode.indent(state.localState, textAfter);
        },

        compareStates: function (a, b) {
            return htmlMode.compareStates(a.htmlState, b.htmlState);
        },

        electricChars: "/{}:"
    }
});

CodeMirror.defineMIME("text/html", "htmlmixed");


CodeMirror.defineMode("haskell", function (cmCfg, modeCfg) {

    function switchState(source, setState, f) {
        setState(f);
        return f(source, setState);
    }

    // These should all be Unicode extended, as per the Haskell 2010 report
    var smallRE = /[a-z_]/;
    var largeRE = /[A-Z]/;
    var digitRE = /[0-9]/;
    var hexitRE = /[0-9A-Fa-f]/;
    var octitRE = /[0-7]/;
    var idRE = /[a-z_A-Z0-9']/;
    var symbolRE = /[-!#$%&*+.\/<=>?@\\^|~:]/;
    var specialRE = /[(),;[\]`{}]/;
    var whiteCharRE = /[ \t\v\f]/; // newlines are handled in tokenizer

    function normal(source, setState) {
        if (source.eatWhile(whiteCharRE)) {
            return null;
        }

        var ch = source.next();
        if (specialRE.test(ch)) {
            if (ch == '{' && source.eat('-')) {
                var t = "comment";
                if (source.eat('#')) {
                    t = "meta";
                }
                return switchState(source, setState, ncomment(t, 1));
            }
            return null;
        }

        if (ch == '\'') {
            if (source.eat('\\')) {
                source.next();  // should handle other escapes here
            }
            else {
                source.next();
            }
            if (source.eat('\'')) {
                return "string";
            }
            return "error";
        }

        if (ch == '"') {
            return switchState(source, setState, stringLiteral);
        }

        if (largeRE.test(ch)) {
            source.eatWhile(idRE);
            if (source.eat('.')) {
                return "qualifier";
            }
            return "variable-2";
        }

        if (smallRE.test(ch)) {
            source.eatWhile(idRE);
            return "variable";
        }

        if (digitRE.test(ch)) {
            if (ch == '0') {
                if (source.eat(/[xX]/)) {
                    source.eatWhile(hexitRE); // should require at least 1
                    return "integer";
                }
                if (source.eat(/[oO]/)) {
                    source.eatWhile(octitRE); // should require at least 1
                    return "number";
                }
            }
            source.eatWhile(digitRE);
            var t = "number";
            if (source.eat('.')) {
                t = "number";
                source.eatWhile(digitRE); // should require at least 1
            }
            if (source.eat(/[eE]/)) {
                t = "number";
                source.eat(/[-+]/);
                source.eatWhile(digitRE); // should require at least 1
            }
            return t;
        }

        if (symbolRE.test(ch)) {
            if (ch == '-' && source.eat(/-/)) {
                source.eatWhile(/-/);
                if (!source.eat(symbolRE)) {
                    source.skipToEnd();
                    return "comment";
                }
            }
            var t = "variable";
            if (ch == ':') {
                t = "variable-2";
            }
            source.eatWhile(symbolRE);
            return t;
        }

        return "error";
    }

    function ncomment(type, nest) {
        if (nest == 0) {
            return normal;
        }
        return function (source, setState) {
            var currNest = nest;
            while (!source.eol()) {
                var ch = source.next();
                if (ch == '{' && source.eat('-')) {
                    ++currNest;
                }
                else if (ch == '-' && source.eat('}')) {
                    --currNest;
                    if (currNest == 0) {
                        setState(normal);
                        return type;
                    }
                }
            }
            setState(ncomment(type, currNest));
            return type;
        }
    }

    function stringLiteral(source, setState) {
        while (!source.eol()) {
            var ch = source.next();
            if (ch == '"') {
                setState(normal);
                return "string";
            }
            if (ch == '\\') {
                if (source.eol() || source.eat(whiteCharRE)) {
                    setState(stringGap);
                    return "string";
                }
                if (source.eat('&')) {
                }
                else {
                    source.next(); // should handle other escapes here
                }
            }
        }
        setState(normal);
        return "error";
    }

    function stringGap(source, setState) {
        if (source.eat('\\')) {
            return switchState(source, setState, stringLiteral);
        }
        source.next();
        setState(normal);
        return "error";
    }


    var wellKnownWords = (function () {
        var wkw = {};
        function setType(t) {
            return function () {
                for (var i = 0; i < arguments.length; i++)
                    wkw[arguments[i]] = t;
            }
        }

        setType("keyword")(
          "case", "class", "data", "default", "deriving", "do", "else", "foreign",
          "if", "import", "in", "infix", "infixl", "infixr", "instance", "let",
          "module", "newtype", "of", "then", "type", "where", "_");

        setType("keyword")(
          "\.\.", ":", "::", "=", "\\", "\"", "<-", "->", "@", "~", "=>");

        setType("builtin")(
          "!!", "$!", "$", "&&", "+", "++", "-", ".", "/", "/=", "<", "<=", "=<<",
          "==", ">", ">=", ">>", ">>=", "^", "^^", "||", "*", "**");

        setType("builtin")(
          "Bool", "Bounded", "Char", "Double", "EQ", "Either", "Enum", "Eq",
          "False", "FilePath", "Float", "Floating", "Fractional", "Functor", "GT",
          "IO", "IOError", "Int", "Integer", "Integral", "Just", "LT", "Left",
          "Maybe", "Monad", "Nothing", "Num", "Ord", "Ordering", "Rational", "Read",
          "ReadS", "Real", "RealFloat", "RealFrac", "Right", "Show", "ShowS",
          "String", "True");

        setType("builtin")(
          "abs", "acos", "acosh", "all", "and", "any", "appendFile", "asTypeOf",
          "asin", "asinh", "atan", "atan2", "atanh", "break", "catch", "ceiling",
          "compare", "concat", "concatMap", "const", "cos", "cosh", "curry",
          "cycle", "decodeFloat", "div", "divMod", "drop", "dropWhile", "either",
          "elem", "encodeFloat", "enumFrom", "enumFromThen", "enumFromThenTo",
          "enumFromTo", "error", "even", "exp", "exponent", "fail", "filter",
          "flip", "floatDigits", "floatRadix", "floatRange", "floor", "fmap",
          "foldl", "foldl1", "foldr", "foldr1", "fromEnum", "fromInteger",
          "fromIntegral", "fromRational", "fst", "gcd", "getChar", "getContents",
          "getLine", "head", "id", "init", "interact", "ioError", "isDenormalized",
          "isIEEE", "isInfinite", "isNaN", "isNegativeZero", "iterate", "last",
          "lcm", "length", "lex", "lines", "log", "logBase", "lookup", "map",
          "mapM", "mapM_", "max", "maxBound", "maximum", "maybe", "min", "minBound",
          "minimum", "mod", "negate", "not", "notElem", "null", "odd", "or",
          "otherwise", "pi", "pred", "print", "product", "properFraction",
          "putChar", "putStr", "putStrLn", "quot", "quotRem", "read", "readFile",
          "readIO", "readList", "readLn", "readParen", "reads", "readsPrec",
          "realToFrac", "recip", "rem", "repeat", "replicate", "return", "reverse",
          "round", "scaleFloat", "scanl", "scanl1", "scanr", "scanr1", "seq",
          "sequence", "sequence_", "show", "showChar", "showList", "showParen",
          "showString", "shows", "showsPrec", "significand", "signum", "sin",
          "sinh", "snd", "span", "splitAt", "sqrt", "subtract", "succ", "sum",
          "tail", "take", "takeWhile", "tan", "tanh", "toEnum", "toInteger",
          "toRational", "truncate", "uncurry", "undefined", "unlines", "until",
          "unwords", "unzip", "unzip3", "userError", "words", "writeFile", "zip",
          "zip3", "zipWith", "zipWith3");

        return wkw;
    })();



    return {
        startState: function () { return { f: normal }; },
        copyState: function (s) { return { f: s.f }; },

        token: function (stream, state) {
            var t = state.f(stream, function (s) { state.f = s; });
            var w = stream.current();
            return (w in wellKnownWords) ? wellKnownWords[w] : t;
        }
    };

});

CodeMirror.defineMIME("text/x-haskell", "haskell");
CodeMirror.defineMode("groovy", function(config, parserConfig) {
    function words(str) {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
        return obj;
    }
    var keywords = words(
      "abstract as assert boolean break byte case catch char class const continue def default " +
      "do double else enum extends final finally float for goto if implements import in " +
      "instanceof int interface long native new package private protected public return " +
      "short static strictfp super switch synchronized threadsafe throw throws transient " +
      "try void volatile while");
    var blockKeywords = words("catch class do else finally for if switch try while enum interface def");
    var atoms = words("null true false this");

    var curPunc;
    function tokenBase(stream, state) {
        var ch = stream.next();
        if (ch == '"' || ch == "'") {
            return startString(ch, stream, state);
        }
        if (/[\[\]{}\(\),;\:\.]/.test(ch)) {
            curPunc = ch;
            return null
        }
        if (/\d/.test(ch)) {
            stream.eatWhile(/[\w\.]/);
            if (stream.eat(/eE/)) { stream.eat(/\+\-/); stream.eatWhile(/\d/); }
            return "number";
        }
        if (ch == "/") {
            if (stream.eat("*")) {
                state.tokenize.push(tokenComment);
                return tokenComment(stream, state);
            }
            if (stream.eat("/")) {
                stream.skipToEnd();
                return "comment";
            }
            if (expectExpression(state.lastToken)) {
                return startString(ch, stream, state);
            }
        }
        if (ch == "-" && stream.eat(">")) {
            curPunc = "->";
            return null;
        }
        if (/[+\-*&%=<>!?|\/~]/.test(ch)) {
            stream.eatWhile(/[+\-*&%=<>|~]/);
            return "operator";
        }
        stream.eatWhile(/[\w\$_]/);
        if (ch == "@") return "meta";
        if (state.lastToken == ".") return "property";
        if (stream.eat(":")) { curPunc = "proplabel"; return "property"; }
        var cur = stream.current();
        if (atoms.propertyIsEnumerable(cur)) { return "atom"; }
        if (keywords.propertyIsEnumerable(cur)) {
            if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
            return "keyword";
        }
        return "word";
    }
    tokenBase.isBase = true;

    function startString(quote, stream, state) {
        var tripleQuoted = false;
        if (quote != "/" && stream.eat(quote)) {
            if (stream.eat(quote)) tripleQuoted = true;
            else return "string";
        }
        function t(stream, state) {
            var escaped = false, next, end = !tripleQuoted;
            while ((next = stream.next()) != null) {
                if (next == quote && !escaped) {
                    if (!tripleQuoted) { break; }
                    if (stream.match(quote + quote)) { end = true; break; }
                }
                if (quote == '"' && next == "$" && !escaped && stream.eat("{")) {
                    state.tokenize.push(tokenBaseUntilBrace());
                    return "string";
                }
                escaped = !escaped && next == "\\";
            }
            if (end) state.tokenize.pop();
            return "string";
        }
        state.tokenize.push(t);
        return t(stream, state);
    }

    function tokenBaseUntilBrace() {
        var depth = 1;
        function t(stream, state) {
            if (stream.peek() == "}") {
                depth--;
                if (depth == 0) {
                    state.tokenize.pop();
                    return state.tokenize[state.tokenize.length-1](stream, state);
                }
            } else if (stream.peek() == "{") {
                depth++;
            }
            return tokenBase(stream, state);
        }
        t.isBase = true;
        return t;
    }

    function tokenComment(stream, state) {
        var maybeEnd = false, ch;
        while (ch = stream.next()) {
            if (ch == "/" && maybeEnd) {
                state.tokenize.pop();
                break;
            }
            maybeEnd = (ch == "*");
        }
        return "comment";
    }

    function expectExpression(last) {
        return !last || last == "operator" || last == "->" || /[\.\[\{\(,;:]/.test(last) ||
          last == "newstatement" || last == "keyword" || last == "proplabel";
    }

    function Context(indented, column, type, align, prev) {
        this.indented = indented;
        this.column = column;
        this.type = type;
        this.align = align;
        this.prev = prev;
    }
    function pushContext(state, col, type) {
        return state.context = new Context(state.indented, col, type, null, state.context);
    }
    function popContext(state) {
        var t = state.context.type;
        if (t == ")" || t == "]" || t == "}")
            state.indented = state.context.indented;
        return state.context = state.context.prev;
    }

    // Interface

    return {
        startState: function(basecolumn) {
            return {
                tokenize: [tokenBase],
                context: new Context((basecolumn || 0) - config.indentUnit, 0, "top", false),
                indented: 0,
                startOfLine: true,
                lastToken: null
            };
        },

        token: function(stream, state) {
            var ctx = state.context;
            if (stream.sol()) {
                if (ctx.align == null) ctx.align = false;
                state.indented = stream.indentation();
                state.startOfLine = true;
                // Automatic semicolon insertion
                if (ctx.type == "statement" && !expectExpression(state.lastToken)) {
                    popContext(state); ctx = state.context;
                }
            }
            if (stream.eatSpace()) return null;
            curPunc = null;
            var style = state.tokenize[state.tokenize.length-1](stream, state);
            if (style == "comment") return style;
            if (ctx.align == null) ctx.align = true;

            if ((curPunc == ";" || curPunc == ":") && ctx.type == "statement") popContext(state);
                // Handle indentation for {x -> \n ... }
            else if (curPunc == "->" && ctx.type == "statement" && ctx.prev.type == "}") {
                popContext(state);
                state.context.align = false;
            }
            else if (curPunc == "{") pushContext(state, stream.column(), "}");
            else if (curPunc == "[") pushContext(state, stream.column(), "]");
            else if (curPunc == "(") pushContext(state, stream.column(), ")");
            else if (curPunc == "}") {
                while (ctx.type == "statement") ctx = popContext(state);
                if (ctx.type == "}") ctx = popContext(state);
                while (ctx.type == "statement") ctx = popContext(state);
            }
            else if (curPunc == ctx.type) popContext(state);
            else if (ctx.type == "}" || ctx.type == "top" || (ctx.type == "statement" && curPunc == "newstatement"))
                pushContext(state, stream.column(), "statement");
            state.startOfLine = false;
            state.lastToken = curPunc || style;
            return style;
        },

        indent: function(state, textAfter) {
            if (!state.tokenize[state.tokenize.length-1].isBase) return 0;
            var firstChar = textAfter && textAfter.charAt(0), ctx = state.context;
            if (ctx.type == "statement" && !expectExpression(state.lastToken)) ctx = ctx.prev;
            var closing = firstChar == ctx.type;
            if (ctx.type == "statement") return ctx.indented + (firstChar == "{" ? 0 : config.indentUnit);
            else if (ctx.align) return ctx.column + (closing ? 0 : 1);
            else return ctx.indented + (closing ? 0 : config.indentUnit);
        },

        electricChars: "{}"
    };
});

CodeMirror.defineMIME("text/x-groovy", "groovy");
CodeMirror.defineMode("diff", function() {
    return {
        token: function(stream) {
            var ch = stream.next();
            stream.skipToEnd();
            if (ch == "+") return "plus";
            if (ch == "-") return "minus";
            if (ch == "@") return "rangeinfo";
        }
    };
});

CodeMirror.defineMIME("text/x-diff", "diff");
CodeMirror.defineMode("css", function(config) {
    var indentUnit = config.indentUnit, type;
    function ret(style, tp) {type = tp; return style;}

    function tokenBase(stream, state) {
        var ch = stream.next();
        if (ch == "@") {stream.eatWhile(/[\w\\\-]/); return ret("meta", stream.current());}
        else if (ch == "/" && stream.eat("*")) {
            state.tokenize = tokenCComment;
            return tokenCComment(stream, state);
        }
        else if (ch == "<" && stream.eat("!")) {
            state.tokenize = tokenSGMLComment;
            return tokenSGMLComment(stream, state);
        }
        else if (ch == "=") ret(null, "compare");
        else if ((ch == "~" || ch == "|") && stream.eat("=")) return ret(null, "compare");
        else if (ch == "\"" || ch == "'") {
            state.tokenize = tokenString(ch);
            return state.tokenize(stream, state);
        }
        else if (ch == "#") {
            stream.eatWhile(/[\w\\\-]/);
            return ret("atom", "hash");
        }
        else if (ch == "!") {
            stream.match(/^\s*\w*/);
            return ret("keyword", "important");
        }
        else if (/\d/.test(ch)) {
            stream.eatWhile(/[\w.%]/);
            return ret("number", "unit");
        }
        else if (/[,.+>*\/]/.test(ch)) {
            return ret(null, "select-op");
        }
        else if (/[;{}:\[\]]/.test(ch)) {
            return ret(null, ch);
        }
        else {
            stream.eatWhile(/[\w\\\-]/);
            return ret("variable", "variable");
        }
    }

    function tokenCComment(stream, state) {
        var maybeEnd = false, ch;
        while ((ch = stream.next()) != null) {
            if (maybeEnd && ch == "/") {
                state.tokenize = tokenBase;
                break;
            }
            maybeEnd = (ch == "*");
        }
        return ret("comment", "comment");
    }

    function tokenSGMLComment(stream, state) {
        var dashes = 0, ch;
        while ((ch = stream.next()) != null) {
            if (dashes >= 2 && ch == ">") {
                state.tokenize = tokenBase;
                break;
            }
            dashes = (ch == "-") ? dashes + 1 : 0;
        }
        return ret("comment", "comment");
    }

    function tokenString(quote) {
        return function(stream, state) {
            var escaped = false, ch;
            while ((ch = stream.next()) != null) {
                if (ch == quote && !escaped)
                    break;
                escaped = !escaped && ch == "\\";
            }
            if (!escaped) state.tokenize = tokenBase;
            return ret("string", "string");
        };
    }

    return {
        startState: function(base) {
            return {tokenize: tokenBase,
                baseIndent: base || 0,
                stack: []};
        },

        token: function(stream, state) {
            if (stream.eatSpace()) return null;
            var style = state.tokenize(stream, state);

            var context = state.stack[state.stack.length-1];
            if (type == "hash" && context == "rule") style = "atom";
            else if (style == "variable") {
                if (context == "rule") style = "number";
                else if (!context || context == "@media{") style = "tag";
            }

            if (context == "rule" && /^[\{\};]$/.test(type))
                state.stack.pop();
            if (type == "{") {
                if (context == "@media") state.stack[state.stack.length-1] = "@media{";
                else state.stack.push("{");
            }
            else if (type == "}") state.stack.pop();
            else if (type == "@media") state.stack.push("@media");
            else if (context == "{" && type != "comment") state.stack.push("rule");
            return style;
        },

        indent: function(state, textAfter) {
            var n = state.stack.length;
            if (/^\}/.test(textAfter))
                n -= state.stack[state.stack.length-1] == "rule" ? 2 : 1;
            return state.baseIndent + n * indentUnit;
        },

        electricChars: "}"
    };
});

CodeMirror.defineMIME("text/css", "css");
/**
 * Link to the project's GitHub page:
 * https://github.com/pickhardt/coffeescript-codemirror-mode
 */
CodeMirror.defineMode('coffeescript', function(conf) {
    var ERRORCLASS = 'error';
    
    function wordRegexp(words) {
        return new RegExp("^((" + words.join(")|(") + "))\\b");
    }
    
    var singleOperators = new RegExp("^[\\+\\-\\*/%&|\\^~<>!\?]");
    var singleDelimiters = new RegExp('^[\\(\\)\\[\\]\\{\\}@,:`=;\\.]');
    var doubleOperators = new RegExp("^((\->)|(\=>)|(\\+\\+)|(\\+\\=)|(\\-\\-)|(\\-\\=)|(\\*\\*)|(\\*\\=)|(\\/\\/)|(\\/\\=)|(==)|(!=)|(<=)|(>=)|(<>)|(<<)|(>>)|(//))");
    var doubleDelimiters = new RegExp("^((\\.\\.)|(\\+=)|(\\-=)|(\\*=)|(%=)|(/=)|(&=)|(\\|=)|(\\^=))");
    var tripleDelimiters = new RegExp("^((\\.\\.\\.)|(//=)|(>>=)|(<<=)|(\\*\\*=))");
    var identifiers = new RegExp("^[_A-Za-z][_A-Za-z0-9]*");

    var wordOperators = wordRegexp(['and', 'or', 'not',
                                    'is', 'isnt', 'in',
                                    'instanceof', 'typeof']);
    var indentKeywords = ['for', 'while', 'loop', 'if', 'unless', 'else',
                          'switch', 'try', 'catch', 'finally', 'class'];
    var commonKeywords = ['break', 'by', 'continue', 'debugger', 'delete',
                          'do', 'in', 'of', 'new', 'return', 'then',
                          'this', 'throw', 'when', 'until'];

    var keywords = wordRegexp(indentKeywords.concat(commonKeywords));

    indentKeywords = wordRegexp(indentKeywords);


    var stringPrefixes = new RegExp("^('{3}|\"{3}|['\"])");
    var regexPrefixes = new RegExp("^(/{3}|/)");
    var commonConstants = ['Infinity', 'NaN', 'undefined', 'null', 'true', 'false', 'on', 'off', 'yes', 'no'];
    var constants = wordRegexp(commonConstants);

    // Tokenizers
    function tokenBase(stream, state) {
        // Handle scope changes
        if (stream.sol()) {
            var scopeOffset = state.scopes[0].offset;
            if (stream.eatSpace()) {
                var lineOffset = stream.indentation();
                if (lineOffset > scopeOffset) {
                    return 'indent';
                } else if (lineOffset < scopeOffset) {
                    return 'dedent';
                }
                return null;
            } else {
                if (scopeOffset > 0) {
                    dedent(stream, state);
                }
            }
        }
        if (stream.eatSpace()) {
            return null;
        }
        
        var ch = stream.peek();
        
        // Handle comments
        if (ch === '#') {
            stream.skipToEnd();
            return 'comment';
        }
        
        // Handle number literals
        if (stream.match(/^-?[0-9\.]/, false)) {
            var floatLiteral = false;
            // Floats
            if (stream.match(/^-?\d*\.\d+(e[\+\-]?\d+)?/i)) {
                floatLiteral = true;
            }
            if (stream.match(/^-?\d+\.\d*/)) {
                floatLiteral = true;
            }
            if (stream.match(/^-?\.\d+/)) {
                floatLiteral = true;
            }
            if (floatLiteral) {
                return 'number';
            }
            // Integers
            var intLiteral = false;
            // Hex
            if (stream.match(/^-?0x[0-9a-f]+/i)) {
                intLiteral = true;
            }
            // Decimal
            if (stream.match(/^-?[1-9]\d*(e[\+\-]?\d+)?/)) {
                intLiteral = true;
            }
            // Zero by itself with no other piece of number.
            if (stream.match(/^-?0(?![\dx])/i)) {
                intLiteral = true;
            }
            if (intLiteral) {
                return 'number';
            }
        }
        
        // Handle strings
        if (stream.match(stringPrefixes)) {
            state.tokenize = tokenFactory(stream.current(), 'string');
            return state.tokenize(stream, state);
        }
        // Handle regex literals
        if (stream.match(regexPrefixes)) {
            if (stream.current() != '/' || stream.match(/^.*\//, false)) { // prevent highlight of division
                state.tokenize = tokenFactory(stream.current(), 'string-2');
                return state.tokenize(stream, state);
            } else {
                stream.backUp(1);
            }
        }
        
        // Handle operators and delimiters
        if (stream.match(tripleDelimiters) || stream.match(doubleDelimiters)) {
            return 'punctuation';
        }
        if (stream.match(doubleOperators)
            || stream.match(singleOperators)
            || stream.match(wordOperators)) {
            return 'operator';
        }
        if (stream.match(singleDelimiters)) {
            return 'punctuation';
        }
        
        if (stream.match(constants)) {
            return 'atom';
        }
        
        if (stream.match(keywords)) {
            return 'keyword';
        }
        
        if (stream.match(identifiers)) {
            return 'variable';
        }
        
        // Handle non-detected items
        stream.next();
        return ERRORCLASS;
    }
    
    function tokenFactory(delimiter, outclass) {
        var delim_re = new RegExp(delimiter);
        var singleline = delimiter.length == 1;
        
        return function tokenString(stream, state) {
            while (!stream.eol()) {
                stream.eatWhile(/[^'"\/\\]/);
                if (stream.eat('\\')) {
                    stream.next();
                    if (singleline && stream.eol()) {
                        return outclass;
                    }
                } else if (stream.match(delim_re)) {
                    state.tokenize = tokenBase;
                    return outclass;
                } else {
                    stream.eat(/['"\/]/);
                }
            }
            if (singleline) {
                if (conf.mode.singleLineStringErrors) {
                    outclass = ERRORCLASS
                } else {
                    state.tokenize = tokenBase;
                }
            }
            return outclass;
        };
    }
    
    function indent(stream, state, type) {
        type = type || 'coffee';
        var indentUnit = 0;
        if (type === 'coffee') {
            for (var i = 0; i < state.scopes.length; i++) {
                if (state.scopes[i].type === 'coffee') {
                    indentUnit = state.scopes[i].offset + conf.indentUnit;
                    break;
                }
            }
        } else {
            indentUnit = stream.column() + stream.current().length;
        }
        state.scopes.unshift({
            offset: indentUnit,
            type: type
        });
    }
    
    function dedent(stream, state) {
        if (state.scopes.length == 1) return;
        if (state.scopes[0].type === 'coffee') {
            var _indent = stream.indentation();
            var _indent_index = -1;
            for (var i = 0; i < state.scopes.length; ++i) {
                if (_indent === state.scopes[i].offset) {
                    _indent_index = i;
                    break;
                }
            }
            if (_indent_index === -1) {
                return true;
            }
            while (state.scopes[0].offset !== _indent) {
                state.scopes.shift();
            }
            return false
        } else {
            state.scopes.shift();
            return false;
        }
    }

    function tokenLexer(stream, state) {
        var style = state.tokenize(stream, state);
        var current = stream.current();

        // Handle '.' connected identifiers
        if (current === '.') {
            style = state.tokenize(stream, state);
            current = stream.current();
            if (style === 'variable') {
                return 'variable';
            } else {
                return ERRORCLASS;
            }
        }
        
        // Handle properties
        if (current === '@') {
            style = state.tokenize(stream, state);
            current = stream.current();
            if (style === 'variable') {
                return 'variable-2';
            } else {
                return ERRORCLASS;
            }
        }
        
        // Handle scope changes.
        if (current === 'return') {
            state.dedent += 1;
        }
        if (((current === '->' || current === '=>') &&
                  !state.lambda &&
                  state.scopes[0].type == 'coffee' &&
                  stream.peek() === '')
               || style === 'indent') {
            indent(stream, state);
        }
        var delimiter_index = '[({'.indexOf(current);
        if (delimiter_index !== -1) {
            indent(stream, state, '])}'.slice(delimiter_index, delimiter_index+1));
        }
        if (indentKeywords.exec(current)){
            indent(stream, state);
        }
        if (current == 'then'){
            dedent(stream, state);
        }
        

        if (style === 'dedent') {
            if (dedent(stream, state)) {
                return ERRORCLASS;
            }
        }
        delimiter_index = '])}'.indexOf(current);
        if (delimiter_index !== -1) {
            if (dedent(stream, state)) {
                return ERRORCLASS;
            }
        }
        if (state.dedent > 0 && stream.eol() && state.scopes[0].type == 'coffee') {
            if (state.scopes.length > 1) state.scopes.shift();
            state.dedent -= 1;
        }
        
        return style;
    }

    var external = {
        startState: function(basecolumn) {
            return {
                tokenize: tokenBase,
                scopes: [{offset:basecolumn || 0, type:'coffee'}],
                lastToken: null,
                lambda: false,
                dedent: 0
            };
        },
        
        token: function(stream, state) {
            var style = tokenLexer(stream, state);
            
            state.lastToken = {style:style, content: stream.current()};
            
            if (stream.eol() && stream.lambda) {
                state.lambda = false;
            }
            
            return style;
        },
        
        indent: function(state, textAfter) {
            if (state.tokenize != tokenBase) {
                return 0;
            }
            
            return state.scopes[0].offset;
        }
        
    };
    return external;
});

CodeMirror.defineMIME('text/x-coffeescript', 'coffeescript');
/**
 * Author: Hans Engel
 * Branched from CodeMirror's Scheme mode (by Koh Zi Han, based on implementation by Koh Zi Chun)
 */
CodeMirror.defineMode("clojure", function (config, mode) {
    var BUILTIN = "builtin", COMMENT = "comment", STRING = "string", TAG = "tag",
        ATOM = "atom", NUMBER = "number", BRACKET = "bracket", KEYWORD="keyword";
    var INDENT_WORD_SKIP = 2, KEYWORDS_SKIP = 1;

    function makeKeywords(str) {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
        return obj;
    }

    var atoms = makeKeywords("true false nil");

    var keywords = makeKeywords(
        // Control structures
        "defn defn- def def- defonce defmulti defmethod defmacro defstruct deftype defprotocol defrecord deftest slice defalias defhinted defmacro- defn-memo defnk defnk defonce- defunbound defunbound- defvar defvar- let letfn do case cond condp for loop recur when when-not when-let when-first if if-let if-not . .. -> ->> doto and or dosync doseq dotimes dorun doall load import unimport ns in-ns refer try catch finally throw with-open with-local-vars binding gen-class gen-and-load-class gen-and-save-class handler-case handle" +

        // Built-ins
        "* *1 *2 *3 *agent* *allow-unresolved-vars* *assert *clojure-version* *command-line-args* *compile-files* *compile-path* *e *err* *file* *flush-on-newline* *in* *macro-meta* *math-context* *ns* *out* *print-dup* *print-length* *print-level* *print-meta* *print-readably* *read-eval* *source-path* *use-context-classloader* *warn-on-reflection* + - / < <= = == > >= accessor aclone agent agent-errors aget alength alias all-ns alter alter-meta! alter-var-root amap ancestors and apply areduce array-map aset aset-boolean aset-byte aset-char aset-double aset-float aset-int aset-long aset-short assert assoc assoc! assoc-in associative? atom await await-for await1 bases bean bigdec bigint binding bit-and bit-and-not bit-clear bit-flip bit-not bit-or bit-set bit-shift-left bit-shift-right bit-test bit-xor boolean boolean-array booleans bound-fn bound-fn* butlast byte byte-array bytes case cast char char-array char-escape-string char-name-string char? chars chunk chunk-append chunk-buffer chunk-cons chunk-first chunk-next chunk-rest chunked-seq? class class? clear-agent-errors clojure-version coll? comment commute comp comparator compare compare-and-set! compile complement concat cond condp conj conj! cons constantly construct-proxy contains? count counted? create-ns create-struct cycle dec decimal? declare definline defmacro defmethod defmulti defn defn- defonce defstruct delay delay? deliver deref derive descendants destructure disj disj! dissoc dissoc! distinct distinct? doall doc dorun doseq dosync dotimes doto double double-array doubles drop drop-last drop-while empty empty? ensure enumeration-seq eval even? every? extend extend-protocol extend-type extends? extenders false? ffirst file-seq filter find find-doc find-ns find-var first float float-array float? floats flush fn fn? fnext for force format future future-call future-cancel future-cancelled? future-done? future? gen-class gen-interface gensym get get-in get-method get-proxy-class get-thread-bindings get-validator hash hash-map hash-set identical? identity if-let if-not ifn? import in-ns inc init-proxy instance? int int-array integer? interleave intern interpose into into-array ints io! isa? iterate iterator-seq juxt key keys keyword keyword? last lazy-cat lazy-seq let letfn line-seq list list* list? load load-file load-reader load-string loaded-libs locking long long-array longs loop macroexpand macroexpand-1 make-array make-hierarchy map map? mapcat max max-key memfn memoize merge merge-with meta method-sig methods min min-key mod name namespace neg? newline next nfirst nil? nnext not not-any? not-empty not-every? not= ns ns-aliases ns-imports ns-interns ns-map ns-name ns-publics ns-refers ns-resolve ns-unalias ns-unmap nth nthnext num number? odd? or parents partial partition pcalls peek persistent! pmap pop pop! pop-thread-bindings pos? pr pr-str prefer-method prefers primitives-classnames print print-ctor print-doc print-dup print-method print-namespace-doc print-simple print-special-doc print-str printf println println-str prn prn-str promise proxy proxy-call-with-super proxy-mappings proxy-name proxy-super push-thread-bindings pvalues quot rand rand-int range ratio? rational? rationalize re-find re-groups re-matcher re-matches re-pattern re-seq read read-line read-string reify reduce ref ref-history-count ref-max-history ref-min-history ref-set refer refer-clojure release-pending-sends rem remove remove-method remove-ns repeat repeatedly replace replicate require reset! reset-meta! resolve rest resultset-seq reverse reversible? rseq rsubseq satisfies? second select-keys send send-off seq seq? seque sequence sequential? set set-validator! set? short short-array shorts shutdown-agents slurp some sort sort-by sorted-map sorted-map-by sorted-set sorted-set-by sorted? special-form-anchor special-symbol? split-at split-with str stream? string? struct struct-map subs subseq subvec supers swap! symbol symbol? sync syntax-symbol-anchor take take-last take-nth take-while test the-ns time to-array to-array-2d trampoline transient tree-seq true? type unchecked-add unchecked-dec unchecked-divide unchecked-inc unchecked-multiply unchecked-negate unchecked-remainder unchecked-subtract underive unquote unquote-splicing update-in update-proxy use val vals var-get var-set var? vary-meta vec vector vector? when when-first when-let when-not while with-bindings with-bindings* with-in-str with-loading-context with-local-vars with-meta with-open with-out-str with-precision xml-seq");

    var indentKeys = makeKeywords(
        // Built-ins
        "ns fn def defn defmethod bound-fn if if-not case condp when while when-not when-first do future comment doto locking proxy with-open with-precision reify deftype defrecord defprotocol extend extend-protocol extend-type try catch" +

        // Binding forms
        "let letfn binding loop for doseq dotimes when-let if-let" +

        // Data structures
        "defstruct struct-map assoc" +

        // clojure.test
        "testing deftest" +

        // contrib
        "handler-case handle dotrace deftrace");

    var tests = {
        digit: /\d/,
        digit_or_colon: /[\d:]/,
        hex: /[0-9a-fA-F]/,
        sign: /[+-]/,
        exponent: /[eE]/,
        keyword_char: /[^\s\(\[\;\)\]]/,
        basic: /[\w\$_\-]/,
        lang_keyword: /[\w*+!\-_?:\/]/
    };

    function stateStack(indent, type, prev) { // represents a state stack object
        this.indent = indent;
        this.type = type;
        this.prev = prev;
    }

    function pushStack(state, indent, type) {
        state.indentStack = new stateStack(indent, type, state.indentStack);
    }

    function popStack(state) {
        state.indentStack = state.indentStack.prev;
    }

    function isNumber(ch, stream){
        // hex
        if ( ch === '0' && 'x' == stream.peek().toLowerCase() ) {
            stream.eat('x');
            stream.eatWhile(tests.hex);
            return true;
        }

        // leading sign
        if ( ch == '+' || ch == '-' ) {
            stream.eat(tests.sign);
            ch = stream.next();
        }

        if ( tests.digit.test(ch) ) {
            stream.eat(ch);
            stream.eatWhile(tests.digit);

            if ( '.' == stream.peek() ) {
                stream.eat('.');
                stream.eatWhile(tests.digit);
            }

            if ( 'e' == stream.peek().toLowerCase() ) {
                stream.eat(tests.exponent);
                stream.eat(tests.sign);
                stream.eatWhile(tests.digit);
            }

            return true;
        }

        return false;
    }

    return {
        startState: function () {
            return {
                indentStack: null,
                indentation: 0,
                mode: false,
            };
        },

        token: function (stream, state) {
            if (state.indentStack == null && stream.sol()) {
                // update indentation, but only if indentStack is empty
                state.indentation = stream.indentation();
            }

            // skip spaces
            if (stream.eatSpace()) {
                return null;
            }
            var returnType = null;

            switch(state.mode){
                case "string": // multi-line string parsing mode
                    var next, escaped = false;
                    while ((next = stream.next()) != null) {
                        if (next == "\"" && !escaped) {

                            state.mode = false;
                            break;
                        }
                        escaped = !escaped && next == "\\";
                    }
                    returnType = STRING; // continue on in string mode
                    break;
                default: // default parsing mode
                    var ch = stream.next();

                    if (ch == "\"") {
                        state.mode = "string";
                        returnType = STRING;
                    } else if (ch == "'" && !( tests.digit_or_colon.test(stream.peek()) )) {
                        returnType = ATOM;
                    } else if (ch == ";") { // comment
                        stream.skipToEnd(); // rest of the line is a comment
                        returnType = COMMENT;
                    } else if (isNumber(ch,stream)){
                        returnType = NUMBER;
                    } else if (ch == "(" || ch == "[") {
                        var keyWord = ''; var indentTemp = stream.column();
                        /**
                        Either
                        (indent-word ..
                        (non-indent-word ..
                        (;something else, bracket, etc.
                        */

                        while ((letter = stream.eat(tests.keyword_char)) != null) {
                            keyWord += letter;
                        }

                        if (keyWord.length > 0 && indentKeys.propertyIsEnumerable(keyWord)) { // indent-word

                            pushStack(state, indentTemp + INDENT_WORD_SKIP, ch);
                        } else { // non-indent word
                            // we continue eating the spaces
                            stream.eatSpace();
                            if (stream.eol() || stream.peek() == ";") {
                                // nothing significant after
                                // we restart indentation 1 space after
                                pushStack(state, indentTemp + 1, ch);
                            } else {
                                pushStack(state, indentTemp + stream.current().length, ch); // else we match
                            }
                        }
                        stream.backUp(stream.current().length - 1); // undo all the eating

                        returnType = BRACKET;
                    } else if (ch == ")" || ch == "]") {
                        returnType = BRACKET;
                        if (state.indentStack != null && state.indentStack.type == (ch == ")" ? "(" : "[")) {
                            popStack(state);
                        }
                    } else if ( ch == ":" ) {
                        stream.eatWhile(tests.lang_keyword);
                        return TAG;
                    } else {
                        stream.eatWhile(tests.basic);

                        if (keywords && keywords.propertyIsEnumerable(stream.current())) {
                            returnType = BUILTIN;
                        } else if ( atoms && atoms.propertyIsEnumerable(stream.current()) ) {
                            returnType = ATOM;
                        } else returnType = null;
                    }
            }

            return returnType;
        },

        indent: function (state, textAfter) {
            if (state.indentStack == null) return state.indentation;
            return state.indentStack.indent;
        }
    };
});

CodeMirror.defineMIME("text/x-clojure", "clojure");

CodeMirror.defineMode("yaml", function() {
	
    var cons = ['true', 'false', 'on', 'off', 'yes', 'no'];
    var keywordRegex = new RegExp("\\b(("+cons.join(")|(")+"))$", 'i');
	
    return {
        token: function(stream, state) {
            var ch = stream.peek();
            var esc = state.escaped;
            state.escaped = false;
            /* comments */
            if (ch == "#") { stream.skipToEnd(); return "comment"; }
            if (state.literal && stream.indentation() > state.keyCol) {
                stream.skipToEnd(); return "string";
            } else if (state.literal) { state.literal = false; }
            if (stream.sol()) {
                state.keyCol = 0;
                state.pair = false;
                state.pairStart = false;
                /* document start */
                if(stream.match(/---/)) { return "def"; }
                /* document end */
                if (stream.match(/\.\.\./)) { return "def"; }
                /* array list item */
                if (stream.match(/\s*-\s+/)) { return 'meta'; }
            }
            /* pairs (associative arrays) -> key */
            if (!state.pair && stream.match(/^\s*([a-z0-9\._-])+(?=\s*:)/i)) {
                state.pair = true;
                state.keyCol = stream.indentation();
                return "atom";
            }
            if (state.pair && stream.match(/^:\s*/)) { state.pairStart = true; return 'meta'; }
			
            /* inline pairs/lists */
            if (stream.match(/^(\{|\}|\[|\])/)) {
                if (ch == '{')
                    state.inlinePairs++;
                else if (ch == '}')
                    state.inlinePairs--;
                else if (ch == '[')
                    state.inlineList++;
                else
                    state.inlineList--;
                return 'meta';
            }
			
            /* list seperator */
            if (state.inlineList > 0 && !esc && ch == ',') {
                stream.next();
                return 'meta';
            }
            /* pairs seperator */
            if (state.inlinePairs > 0 && !esc && ch == ',') {
                state.keyCol = 0;
                state.pair = false;
                state.pairStart = false;
                stream.next();
                return 'meta';
            }
			
            /* start of value of a pair */
            if (state.pairStart) {
                /* block literals */
                if (stream.match(/^\s*(\||\>)\s*/)) { state.literal = true; return 'meta'; };
                /* references */
                if (stream.match(/^\s*(\&|\*)[a-z0-9\._-]+\b/i)) { return 'variable-2'; }
                /* numbers */
                if (state.inlinePairs == 0 && stream.match(/^\s*-?[0-9\.\,]+\s?$/)) { return 'number'; }
                if (state.inlinePairs > 0 && stream.match(/^\s*-?[0-9\.\,]+\s?(?=(,|}))/)) { return 'number'; }
                /* keywords */
                if (stream.match(keywordRegex)) { return 'keyword'; }
            }

            /* nothing found, continue */
            state.pairStart = false;
            state.escaped = (ch == '\\');
            stream.next();
            return null;
        },
        startState: function() {
            return {
                pair: false,
                pairStart: false,
                keyCol: 0,
                inlinePairs: 0,
                inlineList: 0,
                literal: false,
                escaped: false
            };
        }
    };
});

CodeMirror.defineMIME("text/x-yaml", "yaml");

/**
 * xmlpure.js
 * 
 * Building upon and improving the CodeMirror 2 XML parser
 * @author: Dror BG (deebug.dev@gmail.com)
 * @date: August, 2011
 */

CodeMirror.defineMode("xmlpure", function(config, parserConfig) {
    // constants
    var STYLE_ERROR = "error";
    var STYLE_INSTRUCTION = "comment";
    var STYLE_COMMENT = "comment";
    var STYLE_ELEMENT_NAME = "tag";
    var STYLE_ATTRIBUTE = "attribute";
    var STYLE_WORD = "string";
    var STYLE_TEXT = "atom";

    var TAG_INSTRUCTION = "!instruction";
    var TAG_CDATA = "!cdata";
    var TAG_COMMENT = "!comment";
    var TAG_TEXT = "!text";
    
    var doNotIndent = {
        "!cdata": true,
        "!comment": true,
        "!text": true,
        "!instruction": true
    };

    // options
    var indentUnit = config.indentUnit;

    ///////////////////////////////////////////////////////////////////////////
    // helper functions
    
    // chain a parser to another parser
    function chain(stream, state, parser) {
        state.tokenize = parser;
        return parser(stream, state);
    }
    
    // parse a block (comment, CDATA or text)
    function inBlock(style, terminator, nextTokenize) {
        return function(stream, state) {
            while (!stream.eol()) {
                if (stream.match(terminator)) {
                    popContext(state);
                    state.tokenize = nextTokenize;
                    break;
                }
                stream.next();
            }
            return style;
        };
    }
    
    // go down a level in the document
    // (hint: look at who calls this function to know what the contexts are)
    function pushContext(state, tagName) {
        var noIndent = doNotIndent.hasOwnProperty(tagName) || (state.context && state.context.doIndent);
        var newContext = {
            tagName: tagName,
            prev: state.context,
            indent: state.context ? state.context.indent + indentUnit : 0,
            lineNumber: state.lineNumber,
            indented: state.indented,
            noIndent: noIndent
        };
        state.context = newContext;
    }
    
    // go up a level in the document
    function popContext(state) {
        if (state.context) {
            var oldContext = state.context;
            state.context = oldContext.prev;
            return oldContext;
        }
        
        // we shouldn't be here - it means we didn't have a context to pop
        return null;
    }
    
    // return true if the current token is seperated from the tokens before it
    // which means either this is the start of the line, or there is at least
    // one space or tab character behind the token
    // otherwise returns false
    function isTokenSeparated(stream) {
        return stream.sol() ||
            stream.string.charAt(stream.start - 1) == " " ||
            stream.string.charAt(stream.start - 1) == "\t";
    }
    
    ///////////////////////////////////////////////////////////////////////////
    // context: document
    // 
    // an XML document can contain:
    // - a single declaration (if defined, it must be the very first line)
    // - exactly one root element
    // @todo try to actually limit the number of root elements to 1
    // - zero or more comments
    function parseDocument(stream, state) {
        if(stream.eat("<")) {
            if(stream.eat("?")) {
                // processing instruction
                pushContext(state, TAG_INSTRUCTION);
                state.tokenize = parseProcessingInstructionStartTag;
                return STYLE_INSTRUCTION;
            } else if(stream.match("!--")) {
                // new context: comment
                pushContext(state, TAG_COMMENT);
                return chain(stream, state, inBlock(STYLE_COMMENT, "-->", parseDocument));
            } else if(stream.eatSpace() || stream.eol() ) {
                stream.skipToEnd();
                return STYLE_ERROR;
            } else {
                // element
                state.tokenize = parseElementTagName;
                return STYLE_ELEMENT_NAME;
            }
        }
        
        // error on line
        stream.skipToEnd();
        return STYLE_ERROR;
    }

    ///////////////////////////////////////////////////////////////////////////
    // context: XML element start-tag or end-tag
    //
    // - element start-tag can contain attributes
    // - element start-tag may self-close (or start an element block if it doesn't)
    // - element end-tag can contain only the tag name
    function parseElementTagName(stream, state) {
        // get the name of the tag
        var startPos = stream.pos;
        if(stream.match(/^[a-zA-Z_:][-a-zA-Z0-9_:.]*/)) {
            // element start-tag
            var tagName = stream.string.substring(startPos, stream.pos);
            pushContext(state, tagName);
            state.tokenize = parseElement;
            return STYLE_ELEMENT_NAME;
        } else if(stream.match(/^\/[a-zA-Z_:][-a-zA-Z0-9_:.]*( )*>/)) {
            // element end-tag
            var endTagName = stream.string.substring(startPos + 1, stream.pos - 1).trim();
            var oldContext = popContext(state);
            state.tokenize = state.context == null ? parseDocument : parseElementBlock;
            if(oldContext == null || endTagName != oldContext.tagName) {
                // the start and end tag names should match - error
                return STYLE_ERROR;
            }
            return STYLE_ELEMENT_NAME;
        } else {
            // no tag name - error
            state.tokenize = state.context == null ? parseDocument : parseElementBlock;
            stream.eatWhile(/[^>]/);
            stream.eat(">");
            return STYLE_ERROR;
        }
        
        stream.skipToEnd();
        return null;
    }
    
    function parseElement(stream, state) {
        if(stream.match(/^\/>/)) {
            // self-closing tag
            popContext(state);
            state.tokenize = state.context == null ? parseDocument : parseElementBlock;
            return STYLE_ELEMENT_NAME;
        } else if(stream.eat(/^>/)) {
            state.tokenize = parseElementBlock;
            return STYLE_ELEMENT_NAME;
        } else if(isTokenSeparated(stream) && stream.match(/^[a-zA-Z_:][-a-zA-Z0-9_:.]*( )*=/)) {
            // attribute
            state.tokenize = parseAttribute;
            return STYLE_ATTRIBUTE;
        }
        
        // no other options - this is an error
        state.tokenize = state.context == null ? parseDocument : parseDocument;
        stream.eatWhile(/[^>]/);
        stream.eat(">");
        return STYLE_ERROR;
    }
    
    ///////////////////////////////////////////////////////////////////////////
    // context: attribute
    // 
    // attribute values may contain everything, except:
    // - the ending quote (with ' or ") - this marks the end of the value
    // - the character "<" - should never appear
    // - ampersand ("&") - unless it starts a reference: a string that ends with a semi-colon (";")
    // ---> note: this parser is lax in what may be put into a reference string,
    // ---> consult http://www.w3.org/TR/REC-xml/#NT-Reference if you want to make it tighter
    function parseAttribute(stream, state) {
        var quote = stream.next();
        if(quote != "\"" && quote != "'") {
            // attribute must be quoted
            stream.skipToEnd();
            state.tokenize = parseElement;
            return STYLE_ERROR;
        }
        
        state.tokParams.quote = quote;    
        state.tokenize = parseAttributeValue;
        return STYLE_WORD;
    }

    // @todo: find out whether this attribute value spans multiple lines,
    //        and if so, push a context for it in order not to indent it
    //        (or something of the sort..)
    function parseAttributeValue(stream, state) {
        var ch = "";
        while(!stream.eol()) {
            ch = stream.next();
            if(ch == state.tokParams.quote) {
                // end quote found
                state.tokenize = parseElement;
                return STYLE_WORD;
            } else if(ch == "<") {
                // can't have less-than signs in an attribute value, ever
                stream.skipToEnd()
                state.tokenize = parseElement;
                return STYLE_ERROR;
            } else if(ch == "&") {
                // reference - look for a semi-colon, or return error if none found
                ch = stream.next();
                
                // make sure that semi-colon isn't right after the ampersand
                if(ch == ';') {
                    stream.skipToEnd()
                    state.tokenize = parseElement;
                    return STYLE_ERROR;
                }
                
                // make sure no less-than characters slipped in
                while(!stream.eol() && ch != ";") {
                    if(ch == "<") {
                        // can't have less-than signs in an attribute value, ever
                        stream.skipToEnd()
                        state.tokenize = parseElement;
                        return STYLE_ERROR;
                    }
                    ch = stream.next();
                }
                if(stream.eol() && ch != ";") {
                    // no ampersand found - error
                    stream.skipToEnd();
                    state.tokenize = parseElement;
                    return STYLE_ERROR;
                }                
            }
        }
        
        // attribute value continues to next line
        return STYLE_WORD;
    }
    
    ///////////////////////////////////////////////////////////////////////////
    // context: element block
    //
    // a block can contain:
    // - elements
    // - text
    // - CDATA sections
    // - comments
    function parseElementBlock(stream, state) {
        if(stream.eat("<")) {
            if(stream.match("?")) {
                pushContext(state, TAG_INSTRUCTION);
                state.tokenize = parseProcessingInstructionStartTag;
                return STYLE_INSTRUCTION;
            } else if(stream.match("!--")) {
                // new context: comment
                pushContext(state, TAG_COMMENT);
                return chain(stream, state, inBlock(STYLE_COMMENT, "-->",
                    state.context == null ? parseDocument : parseElementBlock));
            } else if(stream.match("![CDATA[")) {
                // new context: CDATA section
                pushContext(state, TAG_CDATA);
                return chain(stream, state, inBlock(STYLE_TEXT, "]]>",
                    state.context == null ? parseDocument : parseElementBlock));
            } else if(stream.eatSpace() || stream.eol() ) {
                stream.skipToEnd();
                return STYLE_ERROR;
            } else {
                // element
                state.tokenize = parseElementTagName;
                return STYLE_ELEMENT_NAME;
            }
        } else {
            // new context: text
            pushContext(state, TAG_TEXT);
            state.tokenize = parseText;
            return null;
        }
        
        state.tokenize = state.context == null ? parseDocument : parseElementBlock;
        stream.skipToEnd();
        return null;
    }
    
    function parseText(stream, state) {
        stream.eatWhile(/[^<]/);
        if(!stream.eol()) {
            // we cannot possibly be in the document context,
            // just inside an element block
            popContext(state);
            state.tokenize = parseElementBlock;
        }
        return STYLE_TEXT;
    }

    ///////////////////////////////////////////////////////////////////////////
    // context: XML processing instructions
    //
    // XML processing instructions (PIs) allow documents to contain instructions for applications.
    // PI format: <?name data?>
    // - 'name' can be anything other than 'xml' (case-insensitive)
    // - 'data' can be anything which doesn't contain '?>'
    // XML declaration is a special PI (see XML declaration context below)
    function parseProcessingInstructionStartTag(stream, state) {
        if(stream.match("xml", true, true)) {
            // xml declaration
            if(state.lineNumber > 1 || stream.pos > 5) {
                state.tokenize = parseDocument;
                stream.skipToEnd();
                return STYLE_ERROR;
            } else {
                state.tokenize = parseDeclarationVersion;
                return STYLE_INSTRUCTION;
            }
        }

        // regular processing instruction
        if(isTokenSeparated(stream) || stream.match("?>")) {
            // we have a space after the start-tag, or nothing but the end-tag
            // either way - error!
            state.tokenize = parseDocument;
            stream.skipToEnd();
            return STYLE_ERROR;
        }

        state.tokenize = parseProcessingInstructionBody;
        return STYLE_INSTRUCTION;
    }

    function parseProcessingInstructionBody(stream, state) {
        stream.eatWhile(/[^?]/);
        if(stream.eat("?")) {
            if(stream.eat(">")) {
                popContext(state);
                state.tokenize = state.context == null ? parseDocument : parseElementBlock;
            }
        }
        return STYLE_INSTRUCTION;
    }

    
    ///////////////////////////////////////////////////////////////////////////
    // context: XML declaration
    //
    // XML declaration is of the following format:
    // <?xml version="1.0" encoding="UTF-8" standalone="no" ?>
    // - must start at the first character of the first line
    // - may span multiple lines
    // - must include 'version'
    // - may include 'encoding' and 'standalone' (in that order after 'version')
    // - attribute names must be lowercase
    // - cannot contain anything else on the line
    function parseDeclarationVersion(stream, state) {
        state.tokenize = parseDeclarationEncoding;
        
        if(isTokenSeparated(stream) && stream.match(/^version( )*=( )*"([a-zA-Z0-9_.:]|\-)+"/)) {
            return STYLE_INSTRUCTION;
        }
        stream.skipToEnd();
        return STYLE_ERROR;
    }

    function parseDeclarationEncoding(stream, state) {
        state.tokenize = parseDeclarationStandalone;
        
        if(isTokenSeparated(stream) && stream.match(/^encoding( )*=( )*"[A-Za-z]([A-Za-z0-9._]|\-)*"/)) {
            return STYLE_INSTRUCTION;
        }
        return null;
    }

    function parseDeclarationStandalone(stream, state) {
        state.tokenize = parseDeclarationEndTag;
        
        if(isTokenSeparated(stream) && stream.match(/^standalone( )*=( )*"(yes|no)"/)) {
            return STYLE_INSTRUCTION;
        }
        return null;
    }

    function parseDeclarationEndTag(stream, state) {
        state.tokenize = parseDocument;
        
        if(stream.match("?>") && stream.eol()) {
            popContext(state);
            return STYLE_INSTRUCTION;
        }
        stream.skipToEnd();
        return STYLE_ERROR;
    }

    ///////////////////////////////////////////////////////////////////////////
    // returned object
    return {
        electricChars: "/",
        
        startState: function() {
            return {
                tokenize: parseDocument,
                tokParams: {},
                lineNumber: 0,
                lineError: false,
                context: null,
                indented: 0
            };
        },

        token: function(stream, state) {
            if(stream.sol()) {
                // initialize a new line
                state.lineNumber++;
                state.lineError = false;
                state.indented = stream.indentation();
            }

            // eat all (the spaces) you can
            if(stream.eatSpace()) return null;

            // run the current tokenize function, according to the state
            var style = state.tokenize(stream, state);
            
            // is there an error somewhere in the line?
            state.lineError = (state.lineError || style == "error");

            return style;
        },
        
        blankLine: function(state) {
            // blank lines are lines too!
            state.lineNumber++;
            state.lineError = false;
        },
        
        indent: function(state, textAfter) {
            if(state.context) {
                if(state.context.noIndent == true) {
                    // do not indent - no return value at all
                    return;
                }
                if(textAfter.match(/^<\/.*/)) {
                    // eng-tag - indent back to last context
                    return state.context.indent;
                }
                // indent to last context + regular indent unit
                return state.context.indent + indentUnit;
            }
            return 0;
        },
        
        compareStates: function(a, b) {
            if (a.indented != b.indented) return false;
            for (var ca = a.context, cb = b.context; ; ca = ca.prev, cb = cb.prev) {
                if (!ca || !cb) return ca == cb;
                if (ca.tagName != cb.tagName) return false;
            }
        }
    };
});

CodeMirror.defineMIME("application/xml", "purexml");
CodeMirror.defineMIME("text/xml", "purexml");

CodeMirror.defineMode("xml", function(config, parserConfig) {
    var indentUnit = config.indentUnit;
    var Kludges = parserConfig.htmlMode ? {
        autoSelfClosers: {"br": true, "img": true, "hr": true, "link": true, "input": true,
            "meta": true, "col": true, "frame": true, "base": true, "area": true},
        doNotIndent: {"pre": true, "!cdata": true},
        allowUnquoted: true
    } : {autoSelfClosers: {}, doNotIndent: {"!cdata": true}, allowUnquoted: false};
    var alignCDATA = parserConfig.alignCDATA;

    // Return variables for tokenizers
    var tagName, type;

    function inText(stream, state) {
        function chain(parser) {
            state.tokenize = parser;
            return parser(stream, state);
        }

        var ch = stream.next();
        if (ch == "<") {
            if (stream.eat("!")) {
                if (stream.eat("[")) {
                    if (stream.match("CDATA[")) return chain(inBlock("atom", "]]>"));
                    else return null;
                }
                else if (stream.match("--")) return chain(inBlock("comment", "-->"));
                else if (stream.match("DOCTYPE", true, true)) {
                    stream.eatWhile(/[\w\._\-]/);
                    return chain(inBlock("meta", ">"));
                }
                else return null;
            }
            else if (stream.eat("?")) {
                stream.eatWhile(/[\w\._\-]/);
                state.tokenize = inBlock("meta", "?>");
                return "meta";
            }
            else {
                type = stream.eat("/") ? "closeTag" : "openTag";
                stream.eatSpace();
                tagName = "";
                var c;
                while ((c = stream.eat(/[^\s\u00a0=<>\"\'\/?]/))) tagName += c;
                state.tokenize = inTag;
                return "tag";
            }
        }
        else if (ch == "&") {
            stream.eatWhile(/[^;]/);
            stream.eat(";");
            return "atom";
        }
        else {
            stream.eatWhile(/[^&<]/);
            return null;
        }
    }

    function inTag(stream, state) {
        var ch = stream.next();
        if (ch == ">" || (ch == "/" && stream.eat(">"))) {
            state.tokenize = inText;
            type = ch == ">" ? "endTag" : "selfcloseTag";
            return "tag";
        }
        else if (ch == "=") {
            type = "equals";
            return null;
        }
        else if (/[\'\"]/.test(ch)) {
            state.tokenize = inAttribute(ch);
            return state.tokenize(stream, state);
        }
        else {
            stream.eatWhile(/[^\s\u00a0=<>\"\'\/?]/);
            return "word";
        }
    }

    function inAttribute(quote) {
        return function(stream, state) {
            while (!stream.eol()) {
                if (stream.next() == quote) {
                    state.tokenize = inTag;
                    break;
                }
            }
            return "string";
        };
    }

    function inBlock(style, terminator) {
        return function(stream, state) {
            while (!stream.eol()) {
                if (stream.match(terminator)) {
                    state.tokenize = inText;
                    break;
                }
                stream.next();
            }
            return style;
        };
    }

    var curState, setStyle;
    function pass() {
        for (var i = arguments.length - 1; i >= 0; i--) curState.cc.push(arguments[i]);
    }
    function cont() {
        pass.apply(null, arguments);
        return true;
    }

    function pushContext(tagName, startOfLine) {
        var noIndent = Kludges.doNotIndent.hasOwnProperty(tagName) || (curState.context && curState.context.noIndent);
        curState.context = {
            prev: curState.context,
            tagName: tagName,
            indent: curState.indented,
            startOfLine: startOfLine,
            noIndent: noIndent
        };
    }
    function popContext() {
        if (curState.context) curState.context = curState.context.prev;
    }

    function element(type) {
        if (type == "openTag") {curState.tagName = tagName; return cont(attributes, endtag(curState.startOfLine));}
        else if (type == "closeTag") {
            var err = false;
            if (curState.context) {
                err = curState.context.tagName != tagName;
            } else {
                err = true;
            }
            if (err) setStyle = "error";
            return cont(endclosetag(err));
        }
        else if (type == "string") {
            if (!curState.context || curState.context.name != "!cdata") pushContext("!cdata");
            if (curState.tokenize == inText) popContext();
            return cont();
        }
        else return cont();
    }
    function endtag(startOfLine) {
        return function(type) {
            if (type == "selfcloseTag" ||
                (type == "endTag" && Kludges.autoSelfClosers.hasOwnProperty(curState.tagName.toLowerCase())))
                return cont();
            if (type == "endTag") {pushContext(curState.tagName, startOfLine); return cont();}
            return cont();
        };
    }
    function endclosetag(err) {
        return function(type) {
            if (err) setStyle = "error";
            if (type == "endTag") { popContext(); return cont(); }
            setStyle = "error";
            return cont(arguments.callee);
        }
    }

    function attributes(type) {
        if (type == "word") {setStyle = "attribute"; return cont(attributes);}
        if (type == "equals") return cont(attvalue, attributes);
        if (type == "string") {setStyle = "error"; return cont(attributes);}
        return pass();
    }
    function attvalue(type) {
        if (type == "word" && Kludges.allowUnquoted) {setStyle = "string"; return cont();}
        if (type == "string") return cont(attvaluemaybe);
        return pass();
    }
    function attvaluemaybe(type) {
        if (type == "string") return cont(attvaluemaybe);
        else return pass();
    }

    return {
        startState: function() {
            return {tokenize: inText, cc: [], indented: 0, startOfLine: true, tagName: null, context: null};
        },

        token: function(stream, state) {
            if (stream.sol()) {
                state.startOfLine = true;
                state.indented = stream.indentation();
            }
            if (stream.eatSpace()) return null;

            setStyle = type = tagName = null;
            var style = state.tokenize(stream, state);
            if ((style || type) && style != "comment") {
                curState = state;
                while (true) {
                    var comb = state.cc.pop() || element;
                    if (comb(type || style)) break;
                }
            }
            state.startOfLine = false;
            return setStyle || style;
        },

        indent: function(state, textAfter) {
            var context = state.context;
            if (context && context.noIndent) return 0;
            if (alignCDATA && /<!\[CDATA\[/.test(textAfter)) return 0;
            if (context && /^<\//.test(textAfter))
                context = context.prev;
            while (context && !context.startOfLine)
                context = context.prev;
            if (context) return context.indent + indentUnit;
            else return 0;
        },

        compareStates: function(a, b) {
            if (a.indented != b.indented) return false;
            for (var ca = a.context, cb = b.context; ; ca = ca.prev, cb = cb.prev) {
                if (!ca || !cb) return ca == cb;
                if (ca.tagName != cb.tagName) return false;
            }
        },

        electricChars: "/"
    };
});

CodeMirror.defineMIME("application/xml", "xml");
CodeMirror.defineMIME("text/html", {name: "xml", htmlMode: true});

CodeMirror.defineMode("velocity", function(config) {
    function parseWords(str) {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
        return obj;
    }

    var indentUnit = config.indentUnit
    var keywords = parseWords("#end #else #break #stop #[[ #]] " +
                              "#{end} #{else} #{break} #{stop}");
    var functions = parseWords("#if #elseif #foreach #set #include #parse #macro #define #evaluate " +
                               "#{if} #{elseif} #{foreach} #{set} #{include} #{parse} #{macro} #{define} #{evaluate}");
    var specials = parseWords("$foreach.count $foreach.hasNext $foreach.first $foreach.last $foreach.topmost $foreach.parent $velocityCount");
    var isOperatorChar = /[+\-*&%=<>!?:\/|]/;
    var multiLineStrings =true;

    function chain(stream, state, f) {
        state.tokenize = f;
        return f(stream, state);
    }
    function tokenBase(stream, state) {
        var beforeParams = state.beforeParams;
        state.beforeParams = false;
        var ch = stream.next();
        // start of string?
        if ((ch == '"' || ch == "'") && state.inParams)
            return chain(stream, state, tokenString(ch));
            // is it one of the special signs []{}().,;? Seperator?
        else if (/[\[\]{}\(\),;\.]/.test(ch)) {
            if (ch == "(" && beforeParams) state.inParams = true;
            else if (ch == ")") state.inParams = false;
            return null;
        }
            // start of a number value?
        else if (/\d/.test(ch)) {
            stream.eatWhile(/[\w\.]/);
            return "number";
        }
            // multi line comment?
        else if (ch == "#" && stream.eat("*")) {
            return chain(stream, state, tokenComment);
        }
            // unparsed content?
        else if (ch == "#" && stream.match(/ *\[ *\[/)) {
            return chain(stream, state, tokenUnparsed);
        }
            // single line comment?
        else if (ch == "#" && stream.eat("#")) {
            stream.skipToEnd();
            return "comment";
        }
            // variable?
        else if (ch == "$") {
            stream.eatWhile(/[\w\d\$_\.{}]/);
            // is it one of the specials?
            if (specials && specials.propertyIsEnumerable(stream.current().toLowerCase())) {
                return "keyword";
            }
            else {
                state.beforeParams = true;
                return "builtin";
            }
        }
            // is it a operator?
        else if (isOperatorChar.test(ch)) {
            stream.eatWhile(isOperatorChar);
            return "operator";
        }
        else {
            // get the whole word
            stream.eatWhile(/[\w\$_{}]/);
            var word = stream.current().toLowerCase();
            // is it one of the listed keywords?
            if (keywords && keywords.propertyIsEnumerable(word))
                return "keyword";
            // is it one of the listed functions?
            if (functions && functions.propertyIsEnumerable(word) ||
                stream.current().match(/^#[a-z0-9_]+ *$/i) && stream.peek()=="(") {
                state.beforeParams = true;
                return "keyword";
            }
            // default: just a "word"
            return null;
        }
    }

    function tokenString(quote) {
        return function(stream, state) {
            var escaped = false, next, end = false;
            while ((next = stream.next()) != null) {
                if (next == quote && !escaped) {
                    end = true;
                    break;
                }
                escaped = !escaped && next == "\\";
            }
            if (end) state.tokenize = tokenBase;
            return "string";
        };
    }

    function tokenComment(stream, state) {
        var maybeEnd = false, ch;
        while (ch = stream.next()) {
            if (ch == "#" && maybeEnd) {
                state.tokenize = tokenBase;
                break;
            }
            maybeEnd = (ch == "*");
        }
        return "comment";
    }

    function tokenUnparsed(stream, state) {
        var maybeEnd = 0, ch;
        while (ch = stream.next()) {
            if (ch == "#" && maybeEnd == 2) {
                state.tokenize = tokenBase;
                break;
            }
            if (ch == "]")
                maybeEnd++;
            else if (ch != " ")
                maybeEnd = 0;
        }
        return "meta";
    }
    // Interface

    return {
        startState: function(basecolumn) {
            return {
                tokenize: tokenBase,
                beforeParams: false,
                inParams: false
            };
        },

        token: function(stream, state) {
            if (stream.eatSpace()) return null;
            return state.tokenize(stream, state);
        }
    };
});

CodeMirror.defineMIME("text/velocity", "velocity");

/***
 |''Name''|tiddlywiki.js|
 |''Description''|Enables TiddlyWikiy syntax highlighting using CodeMirror2|
 |''Author''|PMario|
 |''Version''|0.1.6|
 |''Status''|''beta''|
 |''Source''|[[GitHub|https://github.com/pmario/CodeMirror2/blob/tw-syntax/mode/tiddlywiki]]|
 |''Documentation''|http://codemirror.tiddlyspace.com/|
 |''License''|[[MIT License|http://www.opensource.org/licenses/mit-license.php]]|
 |''CoreVersion''|2.5.0|
 |''Requires''|codemirror.js|
 |''Keywords''|syntax highlighting color code mirror codemirror|
 ! Info
 CoreVersion parameter is needed for TiddlyWiki only!
 ***/
//{{{
CodeMirror.defineMode("tiddlywiki", function (config, parserConfig) {
    var indentUnit = config.indentUnit;

    // Tokenizer
    var textwords = function () {
        function kw(type) {
            return {
                type: type,
                style: "text"
            };
        }
        return {};
    }();

    var keywords = function () {
        function kw(type) {
            return { type: type, style: "macro"};
        }
        return {
            "allTags": kw('allTags'), "closeAll": kw('closeAll'), "list": kw('list'),
            "newJournal": kw('newJournal'), "newTiddler": kw('newTiddler'),
            "permaview": kw('permaview'), "saveChanges": kw('saveChanges'),
            "search": kw('search'), "slider": kw('slider'),	"tabs": kw('tabs'),
            "tag": kw('tag'), "tagging": kw('tagging'),	"tags": kw('tags'),
            "tiddler": kw('tiddler'), "timeline": kw('timeline'),
            "today": kw('today'), "version": kw('version'),	"option": kw('option'),

            "with": kw('with'),
            "filter": kw('filter')
        };
    }();

    var isSpaceName = /[\w_\-]/i,
		reHR = /^\-\-\-\-+$/,
		reWikiCommentStart = /^\/\*\*\*$/,		// /***
		reWikiCommentStop = /^\*\*\*\/$/,		// ***/
		reBlockQuote = /^<<<$/,

		reJsCodeStart = /^\/\/\{\{\{$/,			// //{{{
		reJsCodeStop = /^\/\/\}\}\}$/,			// //}}}
		reXmlCodeStart = /^<!--\{\{\{-->$/,
		reXmlCodeStop = /^<!--\}\}\}-->$/,

		reCodeBlockStart = /^\{\{\{$/,
		reCodeBlockStop = /^\}\}\}$/,

		reCodeStart = /\{\{\{/,
		reUntilCodeStop = /.*?\}\}\}/;

    function chain(stream, state, f) {
        state.tokenize = f;
        return f(stream, state);
    }

    // used for strings
    function nextUntilUnescaped(stream, end) {
        var escaped = false,
			next;
        while ((next = stream.next()) != null) {
            if (next == end && !escaped) return false;
            escaped = !escaped && next == "\\";
        }
        return escaped;
    }

    // Used as scratch variables to communicate multiple values without
    // consing up tons of objects.
    var type, content;

    function ret(tp, style, cont) {
        type = tp;
        content = cont;
        return style;
    }

    function jsTokenBase(stream, state) {
        var sol = stream.sol(), 
			ch, tch;
			
        state.block = false;	// indicates the start of a code block.

        ch = stream.peek(); // don't eat, to make match simpler
		
        // check start of  blocks    
        if (sol && /[<\/\*{}\-]/.test(ch)) {
            if (stream.match(reCodeBlockStart)) {
                state.block = true;
                return chain(stream, state, twTokenCode);
            }
            if (stream.match(reBlockQuote)) {
                return ret('quote', 'quote');
            }
            if (stream.match(reWikiCommentStart) || stream.match(reWikiCommentStop)) {
                return ret('code', 'code');
            }
            if (stream.match(reJsCodeStart) || stream.match(reJsCodeStop) || stream.match(reXmlCodeStart) || stream.match(reXmlCodeStop)) {
                return ret('code', 'code');
            }
            if (stream.match(reHR)) {
                return ret('hr', 'hr');
            }
        } // sol
        var ch = stream.next();

        if (sol && /[\/\*!#;:>|]/.test(ch)) {
            if (ch == "!") { // tw header
                stream.skipToEnd();
                return ret("header", "header");
            }
            if (ch == "*") { // tw list
                stream.eatWhile('*');
                return ret("list", "list");
            }
            if (ch == "#") { // tw numbered list
                stream.eatWhile('#');
                return ret("list", "list");
            }
            if (ch == ";") { // tw list
                stream.eatWhile(';');
                return ret("list", "list");
            }
            if (ch == ":") { // tw list
                stream.eatWhile(':');
                return ret("list", "list");
            }
            if (ch == ">") { // single line quote
                stream.eatWhile(">");
                return ret("quote", "quote");
            }
            if (ch == '|') {
                return ret('table', 'table');
            }
        }

        if (ch == '{' && stream.match(/\{\{/)) {
            return chain(stream, state, twTokenCode);
        }

        // rudimentary html:// file:// link matching. TW knows much more ...
        if (/[hf]/i.test(ch)) {
            if (/[ti]/i.test(stream.peek()) && stream.match(/\b(ttps?|tp|ile):\/\/[\-A-Z0-9+&@#\/%?=~_|$!:,.;]*[A-Z0-9+&@#\/%=~_|$]/i)) {
                return ret("link-external", "link-external");
            }
        }
        // just a little string indicator, don't want to have the whole string covered
        if (ch == '"') {
            return ret('string', 'string');
        }
        if (/[\[\]]/.test(ch)) { // check for [[..]]
            if (stream.peek() == ch) {
                stream.next();
                return ret('brace', 'brace');
            }
        }
        if (ch == "@") {	// check for space link. TODO fix @@...@@ highlighting
            stream.eatWhile(isSpaceName);
            return ret("link-external", "link-external");
        }
        if (/\d/.test(ch)) {	// numbers
            stream.eatWhile(/\d/);
            return ret("number", "number");
        }
        if (ch == "/") { // tw invisible comment
            if (stream.eat("%")) {
                return chain(stream, state, twTokenComment);
            }
            else if (stream.eat("/")) { // 
                return chain(stream, state, twTokenEm);
            }
        }
        if (ch == "_") { // tw underline
            if (stream.eat("_")) {
                return chain(stream, state, twTokenUnderline);
            }
        }
        if (ch == "-") { // tw strikethrough TODO looks ugly .. different handling see below;
            if (stream.eat("-")) {
                return chain(stream, state, twTokenStrike);
            }
        }
        if (ch == "'") { // tw bold
            if (stream.eat("'")) {
                return chain(stream, state, twTokenStrong);
            }
        }
        if (ch == "<") { // tw macro
            if (stream.eat("<")) {
                return chain(stream, state, twTokenMacro);
            }
        }
        else {
            return ret(ch);
        }

        stream.eatWhile(/[\w\$_]/);
        var word = stream.current(),
			known = textwords.propertyIsEnumerable(word) && textwords[word];

        return known ? ret(known.type, known.style, word) : ret("text", null, word);

    } // jsTokenBase()

    function twTokenString(quote) {
        return function (stream, state) {
            if (!nextUntilUnescaped(stream, quote)) state.tokenize = jsTokenBase;
            return ret("string", "string");
        };
    }

    // tw invisible comment
    function twTokenComment(stream, state) {
        var maybeEnd = false,
			ch;
        while (ch = stream.next()) {
            if (ch == "/" && maybeEnd) {
                state.tokenize = jsTokenBase;
                break;
            }
            maybeEnd = (ch == "%");
        }
        return ret("comment", "comment");
    }

    // tw strong / bold
    function twTokenStrong(stream, state) {
        var maybeEnd = false,
			ch;
        while (ch = stream.next()) {
            if (ch == "'" && maybeEnd) {
                state.tokenize = jsTokenBase;
                break;
            }
            maybeEnd = (ch == "'");
        }
        return ret("text", "strong");
    }

    // tw code
    function twTokenCode(stream, state) {
        var ch, sb = state.block;
		
        if (sb && stream.current()) {
            return ret("code", "code");
        }

        if (!sb && stream.match(reUntilCodeStop)) {
            state.tokenize = jsTokenBase;
            return ret("code", "code-inline");
        }

        if (sb && stream.sol() && stream.match(reCodeBlockStop)) {
            state.tokenize = jsTokenBase;
            return ret("code", "code");
        }

        ch = stream.next();
        return (sb) ? ret("code", "code") : ret("code", "code-inline");
    }

    // tw em / italic
    function twTokenEm(stream, state) {
        var maybeEnd = false,
			ch;
        while (ch = stream.next()) {
            if (ch == "/" && maybeEnd) {
                state.tokenize = jsTokenBase;
                break;
            }
            maybeEnd = (ch == "/");
        }
        return ret("text", "em");
    }

    // tw underlined text
    function twTokenUnderline(stream, state) {
        var maybeEnd = false,
			ch;
        while (ch = stream.next()) {
            if (ch == "_" && maybeEnd) {
                state.tokenize = jsTokenBase;
                break;
            }
            maybeEnd = (ch == "_");
        }
        return ret("text", "underlined");
    }

    // tw strike through text looks ugly 
    // TODO just strike through the first and last 2 chars if possible.
    function twTokenStrike(stream, state) {
        var maybeEnd = false,
			ch, nr;
			
        while (ch = stream.next()) {
            if (ch == "-" && maybeEnd) {
                state.tokenize = jsTokenBase;
                break;
            }
            maybeEnd = (ch == "-");
        }
        return ret("text", "line-through");
    }

    // macro
    function twTokenMacro(stream, state) {
        var ch, tmp, word, known;

        if (stream.current() == '<<') {
            return ret('brace', 'macro');
        }

        ch = stream.next();
        if (!ch) {
            state.tokenize = jsTokenBase;
            return ret(ch);
        }
        if (ch == ">") {
            if (stream.peek() == '>') {
                stream.next();
                state.tokenize = jsTokenBase;
                return ret("brace", "macro");
            }
        }

        stream.eatWhile(/[\w\$_]/);
        word = stream.current();
        known = keywords.propertyIsEnumerable(word) && keywords[word];

        if (known) {
            return ret(known.type, known.style, word);
        }
        else {
            return ret("macro", null, word);
        }
    }

    // Interface
    return {
        startState: function (basecolumn) {
            return {
                tokenize: jsTokenBase,
                indented: 0,
                level: 0
            };
        },

        token: function (stream, state) {
            if (stream.eatSpace()) return null;
            var style = state.tokenize(stream, state);
            return style;
        },

        electricChars: ""
    };
});

CodeMirror.defineMIME("text/x-tiddlywiki", "tiddlywiki");
//}}}

/*
 * Author: Constantin Jucovschi (c.jucovschi@jacobs-university.de)
 * Licence: MIT
 */

CodeMirror.defineMode("stex", function(cmCfg, modeCfg) 
{    
    function pushCommand(state, command) {
        state.cmdState.push(command);
    }

    function peekCommand(state) { 
        if (state.cmdState.length>0)
            return state.cmdState[state.cmdState.length-1];
        else
            return null;
    }

    function popCommand(state) {
        if (state.cmdState.length>0) {
            var plug = state.cmdState.pop();
            plug.closeBracket();
        }	    
    }

    function applyMostPowerful(state) {
        var context = state.cmdState;
        for (var i = context.length - 1; i >= 0; i--) {
            var plug = context[i];
            if (plug.name=="DEFAULT")
                continue;
            return plug.styleIdentifier();
        }
        return null;
    }

    function addPluginPattern(pluginName, cmdStyle, brackets, styles) {
        return function () {
            this.name=pluginName;
            this.bracketNo = 0;
            this.style=cmdStyle;
            this.styles = styles;
            this.brackets = brackets;

            this.styleIdentifier = function(content) {
                if (this.bracketNo<=this.styles.length)
                    return this.styles[this.bracketNo-1];
                else
                    return null;
            };
            this.openBracket = function(content) {
                this.bracketNo++;
                return "bracket";
            };
            this.closeBracket = function(content) {
            };
        }
    }

    var plugins = new Array();
   
    plugins["importmodule"] = addPluginPattern("importmodule", "tag", "{[", ["string", "builtin"]);
    plugins["documentclass"] = addPluginPattern("documentclass", "tag", "{[", ["", "atom"]);
    plugins["usepackage"] = addPluginPattern("documentclass", "tag", "[", ["atom"]);
    plugins["begin"] = addPluginPattern("documentclass", "tag", "[", ["atom"]);
    plugins["end"] = addPluginPattern("documentclass", "tag", "[", ["atom"]);

    plugins["DEFAULT"] = function () {
        this.name="DEFAULT";
        this.style="tag";

        this.styleIdentifier = function(content) {
        };
        this.openBracket = function(content) {
        };
        this.closeBracket = function(content) {
        };
    };

    function setState(state, f) {
        state.f = f;
    }

    function normal(source, state) {
        if (source.match(/^\\[a-z]+/)) {
            var cmdName = source.current();
            cmdName = cmdName.substr(1, cmdName.length-1);
            var plug = plugins[cmdName];
            if (typeof(plug) == 'undefined') {
                plug = plugins["DEFAULT"];
            }
            plug = new plug();
            pushCommand(state, plug);
            setState(state, beginParams);
            return plug.style;
        }

        var ch = source.next();
        if (ch == "%") {
            setState(state, inCComment);
            return "comment";
        } 
        else if (ch=='}' || ch==']') {
            plug = peekCommand(state);
            if (plug) {
                plug.closeBracket(ch);
                setState(state, beginParams);
            } else
                return "error";
            return "bracket";
        } else if (ch=='{' || ch=='[') {
            plug = plugins["DEFAULT"];	    
            plug = new plug();
            pushCommand(state, plug);
            return "bracket";	    
        }
        else if (/\d/.test(ch)) {
            source.eatWhile(/[\w.%]/);
            return "atom";
        }
        else {
            source.eatWhile(/[\w-_]/);
            return applyMostPowerful(state);
        }
    }

    function inCComment(source, state) {
        source.skipToEnd();
        setState(state, normal);
        return "comment";
    }

    function beginParams(source, state) {
        var ch = source.peek();
        if (ch == '{' || ch == '[') {
            var lastPlug = peekCommand(state);
            var style = lastPlug.openBracket(ch);
            source.eat(ch);
            setState(state, normal);
            return "bracket";
        }
        if (/[ \t\r]/.test(ch)) {
            source.eat(ch);
            return null;
        }
        setState(state, normal);
        lastPlug = peekCommand(state);
        if (lastPlug) {
            popCommand(state);
        }
        return normal(source, state);
    }

    return {
        startState: function() { return { f:normal, cmdState:[] }; },
        copyState: function(s) { return { f: s.f, cmdState: s.cmdState.slice(0, s.cmdState.length) }; },
	 
        token: function(stream, state) {
            var t = state.f(stream, state);
            var w = stream.current();
            return t;
        }
    };
});


CodeMirror.defineMIME("text/x-stex", "stex");

CodeMirror.defineMode("sparql", function(config) {
    var indentUnit = config.indentUnit;
    var curPunc;

    function wordRegexp(words) {
        return new RegExp("^(?:" + words.join("|") + ")$", "i");
    }
    var ops = wordRegexp(["str", "lang", "langmatches", "datatype", "bound", "sameterm", "isiri", "isuri",
                          "isblank", "isliteral", "union", "a"]);
    var keywords = wordRegexp(["base", "prefix", "select", "distinct", "reduced", "construct", "describe",
                               "ask", "from", "named", "where", "order", "limit", "offset", "filter", "optional",
                               "graph", "by", "asc", "desc"]);
    var operatorChars = /[*+\-<>=&|]/;

    function tokenBase(stream, state) {
        var ch = stream.next();
        curPunc = null;
        if (ch == "$" || ch == "?") {
            stream.match(/^[\w\d]*/);
            return "variable-2";
        }
        else if (ch == "<" && !stream.match(/^[\s\u00a0=]/, false)) {
            stream.match(/^[^\s\u00a0>]*>?/);
            return "atom";
        }
        else if (ch == "\"" || ch == "'") {
            state.tokenize = tokenLiteral(ch);
            return state.tokenize(stream, state);
        }
        else if (/[{}\(\),\.;\[\]]/.test(ch)) {
            curPunc = ch;
            return null;
        }
        else if (ch == "#") {
            stream.skipToEnd();
            return "comment";
        }
        else if (operatorChars.test(ch)) {
            stream.eatWhile(operatorChars);
            return null;
        }
        else if (ch == ":") {
            stream.eatWhile(/[\w\d\._\-]/);
            return "atom";
        }
        else {
            stream.eatWhile(/[_\w\d]/);
            if (stream.eat(":")) {
                stream.eatWhile(/[\w\d_\-]/);
                return "atom";
            }
            var word = stream.current(), type;
            if (ops.test(word))
                return null;
            else if (keywords.test(word))
                return "keyword";
            else
                return "variable";
        }
    }

    function tokenLiteral(quote) {
        return function(stream, state) {
            var escaped = false, ch;
            while ((ch = stream.next()) != null) {
                if (ch == quote && !escaped) {
                    state.tokenize = tokenBase;
                    break;
                }
                escaped = !escaped && ch == "\\";
            }
            return "string";
        };
    }

    function pushContext(state, type, col) {
        state.context = {prev: state.context, indent: state.indent, col: col, type: type};
    }
    function popContext(state) {
        state.indent = state.context.indent;
        state.context = state.context.prev;
    }

    return {
        startState: function(base) {
            return {tokenize: tokenBase,
                context: null,
                indent: 0,
                col: 0};
        },

        token: function(stream, state) {
            if (stream.sol()) {
                if (state.context && state.context.align == null) state.context.align = false;
                state.indent = stream.indentation();
            }
            if (stream.eatSpace()) return null;
            var style = state.tokenize(stream, state);

            if (style != "comment" && state.context && state.context.align == null && state.context.type != "pattern") {
                state.context.align = true;
            }

            if (curPunc == "(") pushContext(state, ")", stream.column());
            else if (curPunc == "[") pushContext(state, "]", stream.column());
            else if (curPunc == "{") pushContext(state, "}", stream.column());
            else if (/[\]\}\)]/.test(curPunc)) {
                while (state.context && state.context.type == "pattern") popContext(state);
                if (state.context && curPunc == state.context.type) popContext(state);
            }
            else if (curPunc == "." && state.context && state.context.type == "pattern") popContext(state);
            else if (/atom|string|variable/.test(style) && state.context) {
                if (/[\}\]]/.test(state.context.type))
                    pushContext(state, "pattern", stream.column());
                else if (state.context.type == "pattern" && !state.context.align) {
                    state.context.align = true;
                    state.context.col = stream.column();
                }
            }
      
            return style;
        },

        indent: function(state, textAfter) {
            var firstChar = textAfter && textAfter.charAt(0);
            var context = state.context;
            if (/[\]\}]/.test(firstChar))
                while (context && context.type == "pattern") context = context.prev;

            var closing = context && firstChar == context.type;
            if (!context)
                return 0;
            else if (context.type == "pattern")
                return context.col;
            else if (context.align)
                return context.col + (closing ? 0 : 1);
            else
                return context.indent + (closing ? 0 : indentUnit);
        }
    };
});

CodeMirror.defineMIME("application/x-sparql-query", "sparql");

CodeMirror.defineMode("smalltalk", function(config, parserConfig) {
    var keywords = {"true": 1, "false": 1, nil: 1, self: 1, "super": 1, thisContext: 1};
    var indentUnit = config.indentUnit;

    function chain(stream, state, f) {
        state.tokenize = f;
        return f(stream, state);
    }

    var type;
    function ret(tp, style) {
        type = tp;
        return style;
    }

    function tokenBase(stream, state) {
        var ch = stream.next();
        if (ch == '"')
            return chain(stream, state, tokenComment(ch));
        else if (ch == "'")
            return chain(stream, state, tokenString(ch));
        else if (ch == "#") {
            stream.eatWhile(/[\w\$_]/);
            return ret("string", "string");
        }
        else if (/\d/.test(ch)) {
            stream.eatWhile(/[\w\.]/)
            return ret("number", "number");
        }
        else if (/[\[\]()]/.test(ch)) {
            return ret(ch, null);
        }
        else {
            stream.eatWhile(/[\w\$_]/);
            if (keywords && keywords.propertyIsEnumerable(stream.current())) return ret("keyword", "keyword");
            return ret("word", "variable");
        }
    }

    function tokenString(quote) {
        return function(stream, state) {
            var escaped = false, next, end = false;
            while ((next = stream.next()) != null) {
                if (next == quote && !escaped) {end = true; break;}
                escaped = !escaped && next == "\\";
            }
            if (end || !(escaped))
                state.tokenize = tokenBase;
            return ret("string", "string");
        };
    }

    function tokenComment(quote) {
        return function(stream, state) {
            var next, end = false;
            while ((next = stream.next()) != null) {
                if (next == quote) {end = true; break;}
            }
            if (end)
                state.tokenize = tokenBase;
            return ret("comment", "comment");
        };
    }

    function Context(indented, column, type, align, prev) {
        this.indented = indented;
        this.column = column;
        this.type = type;
        this.align = align;
        this.prev = prev;
    }

    function pushContext(state, col, type) {
        return state.context = new Context(state.indented, col, type, null, state.context);
    }
    function popContext(state) {
        return state.context = state.context.prev;
    }

    // Interface

    return {
        startState: function(basecolumn) {
            return {
                tokenize: tokenBase,
                context: new Context((basecolumn || 0) - indentUnit, 0, "top", false),
                indented: 0,
                startOfLine: true
            };
        },

        token: function(stream, state) {
            var ctx = state.context;
            if (stream.sol()) {
                if (ctx.align == null) ctx.align = false;
                state.indented = stream.indentation();
                state.startOfLine = true;
            }
            if (stream.eatSpace()) return null;
            var style = state.tokenize(stream, state);
            if (type == "comment") return style;
            if (ctx.align == null) ctx.align = true;

            if (type == "[") pushContext(state, stream.column(), "]");
            else if (type == "(") pushContext(state, stream.column(), ")");
            else if (type == ctx.type) popContext(state);
            state.startOfLine = false;
            return style;
        },

        indent: function(state, textAfter) {
            if (state.tokenize != tokenBase) return 0;
            var firstChar = textAfter && textAfter.charAt(0), ctx = state.context, closing = firstChar == ctx.type;
            if (ctx.align) return ctx.column + (closing ? 0 : 1);
            else return ctx.indented + (closing ? 0 : indentUnit);
        },

        electricChars: "]"
    };
});

CodeMirror.defineMIME("text/x-stsrc", {name: "smalltalk"});

/**
 * Author: Koh Zi Han, based on implementation by Koh Zi Chun
 */
CodeMirror.defineMode("scheme", function (config, mode) {
    var BUILTIN = "builtin", COMMENT = "comment", STRING = "string",
        ATOM = "atom", NUMBER = "number", BRACKET = "bracket", KEYWORD="keyword";
    var INDENT_WORD_SKIP = 2, KEYWORDS_SKIP = 1;
    
    function makeKeywords(str) {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
        return obj;
    }

    var keywords = makeKeywords("λ case-lambda call/cc class define-class exit-handler field import inherit init-field interface let*-values let-values let/ec mixin opt-lambda override protect provide public rename require require-for-syntax syntax syntax-case syntax-error unit/sig unless when with-syntax and begin call-with-current-continuation call-with-input-file call-with-output-file case cond define define-syntax delay do dynamic-wind else for-each if lambda let let* let-syntax letrec letrec-syntax map or syntax-rules abs acos angle append apply asin assoc assq assv atan boolean? caar cadr call-with-input-file call-with-output-file call-with-values car cdddar cddddr cdr ceiling char->integer char-alphabetic? char-ci<=? char-ci<? char-ci=? char-ci>=? char-ci>? char-downcase char-lower-case? char-numeric? char-ready? char-upcase char-upper-case? char-whitespace? char<=? char<? char=? char>=? char>? char? close-input-port close-output-port complex? cons cos current-input-port current-output-port denominator display eof-object? eq? equal? eqv? eval even? exact->inexact exact? exp expt #f floor force gcd imag-part inexact->exact inexact? input-port? integer->char integer? interaction-environment lcm length list list->string list->vector list-ref list-tail list? load log magnitude make-polar make-rectangular make-string make-vector max member memq memv min modulo negative? newline not null-environment null? number->string number? numerator odd? open-input-file open-output-file output-port? pair? peek-char port? positive? procedure? quasiquote quote quotient rational? rationalize read read-char real-part real? remainder reverse round scheme-report-environment set! set-car! set-cdr! sin sqrt string string->list string->number string->symbol string-append string-ci<=? string-ci<? string-ci=? string-ci>=? string-ci>? string-copy string-fill! string-length string-ref string-set! string<=? string<? string=? string>=? string>? string? substring symbol->string symbol? #t tan transcript-off transcript-on truncate values vector vector->list vector-fill! vector-length vector-ref vector-set! with-input-from-file with-output-to-file write write-char zero?");
    var indentKeys = makeKeywords("define let letrec let* lambda");
    

    function stateStack(indent, type, prev) { // represents a state stack object
        this.indent = indent;
        this.type = type;
        this.prev = prev;
    }

    function pushStack(state, indent, type) {
        state.indentStack = new stateStack(indent, type, state.indentStack);
    }

    function popStack(state) {
        state.indentStack = state.indentStack.prev;
    }
    
    /**
     * Scheme numbers are complicated unfortunately.
     * Checks if we're looking at a number, which might be possibly a fraction.
     * Also checks that it is not part of a longer identifier. Returns true/false accordingly.
     */
    function isNumber(ch, stream){ 
        if(/[0-9]/.exec(ch) != null){ 
            stream.eatWhile(/[0-9]/);
            stream.eat(/\//);
            stream.eatWhile(/[0-9]/);
            if (stream.eol() || !(/[a-zA-Z\-\_\/]/.exec(stream.peek()))) return true;
            stream.backUp(stream.current().length - 1); // undo all the eating
        }
        return false;
    }

    return {
        startState: function () {
            return {
                indentStack: null,
                indentation: 0,
                mode: false,
                sExprComment: false
            };
        },

        token: function (stream, state) {
            if (state.indentStack == null && stream.sol()) {
                // update indentation, but only if indentStack is empty
                state.indentation = stream.indentation();
            }

            // skip spaces
            if (stream.eatSpace()) {
                return null;
            }
            var returnType = null;
            
            switch(state.mode){
                case "string": // multi-line string parsing mode
                    var next, escaped = false;
                    while ((next = stream.next()) != null) {
                        if (next == "\"" && !escaped) {
    
                            state.mode = false;
                            break;
                        }
                        escaped = !escaped && next == "\\";
                    }
                    returnType = STRING; // continue on in scheme-string mode
                    break;
                case "comment": // comment parsing mode
                    var next, maybeEnd = false;
                    while ((next = stream.next()) != null) {
                        if (next == "#" && maybeEnd) {
    
                            state.mode = false;
                            break;
                        }
                        maybeEnd = (next == "|");
                    }
                    returnType = COMMENT;
                    break;
                case "s-expr-comment": // s-expr commenting mode
                    state.mode = false;
                    if(stream.peek() == "(" || stream.peek() == "["){
                        // actually start scheme s-expr commenting mode
                        state.sExprComment = 0;
                    }else{
                        // if not we just comment the entire of the next token
                        stream.eatWhile(/[^/s]/); // eat non spaces
                        returnType = COMMENT;
                        break;
                    }
                default: // default parsing mode
                    var ch = stream.next();
        
                    if (ch == "\"") {
                        state.mode = "string";
                        returnType = STRING;
        
                    } else if (ch == "'") {
                        returnType = ATOM;
                    } else if (ch == '#') {
                        if (stream.eat("|")) {					// Multi-line comment
                            state.mode = "comment"; // toggle to comment mode
                            returnType = COMMENT;
                        } else if (stream.eat(/[tf]/)) {			// #t/#f (atom)
                            returnType = ATOM;
                        } else if (stream.eat(';')) {				// S-Expr comment
                            state.mode = "s-expr-comment";
                            returnType = COMMENT;
                        }
        
                    } else if (ch == ";") { // comment
                        stream.skipToEnd(); // rest of the line is a comment
                        returnType = COMMENT;
                    } else if (ch == "-"){
                        
                        if(!isNaN(parseInt(stream.peek()))){
                            stream.eatWhile(/[\/0-9]/);
                            returnType = NUMBER;
                        }else{                            
                            returnType = null;
                        }
                    } else if (isNumber(ch,stream)){
                        returnType = NUMBER;
                    } else if (ch == "(" || ch == "[") {
                        var keyWord = ''; var indentTemp = stream.column();
                        /**
                        Either 
                        (indent-word ..
                        (non-indent-word ..
                        (;something else, bracket, etc.
                        */
        
                        while ((letter = stream.eat(/[^\s\(\[\;\)\]]/)) != null) {
                            keyWord += letter;
                        }
        
                        if (keyWord.length > 0 && indentKeys.propertyIsEnumerable(keyWord)) { // indent-word
        
                            pushStack(state, indentTemp + INDENT_WORD_SKIP, ch);
                        } else { // non-indent word
                            // we continue eating the spaces
                            stream.eatSpace();
                            if (stream.eol() || stream.peek() == ";") {
                                // nothing significant after
                                // we restart indentation 1 space after
                                pushStack(state, indentTemp + 1, ch);
                            } else {
                                pushStack(state, indentTemp + stream.current().length, ch); // else we match
                            }
                        }
                        stream.backUp(stream.current().length - 1); // undo all the eating
                        
                        if(typeof state.sExprComment == "number") state.sExprComment++;
                        
                        returnType = BRACKET;
                    } else if (ch == ")" || ch == "]") {
                        returnType = BRACKET;
                        if (state.indentStack != null && state.indentStack.type == (ch == ")" ? "(" : "[")) {
                            popStack(state);
                            
                            if(typeof state.sExprComment == "number"){
                                if(--state.sExprComment == 0){
                                    returnType = COMMENT; // final closing bracket
                                    state.sExprComment = false; // turn off s-expr commenting mode
                                }
                            }
                        }
                    } else {
                        stream.eatWhile(/[\w\$_\-]/);
        
                        if (keywords && keywords.propertyIsEnumerable(stream.current())) {
                            returnType = BUILTIN;
                        }else returnType = null;
                    }
            }
            return (typeof state.sExprComment == "number") ? COMMENT : returnType;
        },

        indent: function (state, textAfter) {
            if (state.indentStack == null) return state.indentation;
            return state.indentStack.indent;
        }
    };
});

CodeMirror.defineMIME("text/x-scheme", "scheme");

CodeMirror.defineMode("rust", function() {
    var indentUnit = 4, altIndentUnit = 2;
    var valKeywords = {
        "if": "if-style", "while": "if-style", "else": "else-style",
        "do": "else-style", "ret": "else-style", "fail": "else-style",
        "break": "atom", "cont": "atom", "const": "let", "resource": "fn",
        "let": "let", "fn": "fn", "for": "for", "alt": "alt", "obj": "fn",
        "lambda": "fn", "type": "type", "tag": "tag", "mod": "mod",
        "as": "op", "true": "atom", "false": "atom", "assert": "op", "check": "op",
        "claim": "op", "native": "ignore", "unsafe": "ignore", "import": "else-style",
        "export": "else-style", "copy": "op", "log": "op", "log_err": "op"
    };
    var typeKeywords = function() {
        var keywords = {"fn": "fn", "block": "fn", "obj": "obj"};
        var atoms = "bool uint int i8 i16 i32 i64 u8 u16 u32 u64 float f32 f64 str char".split(" ");
        for (var i = 0, e = atoms.length; i < e; ++i) keywords[atoms[i]] = "atom";
        return keywords;
    }();
    var operatorChar = /[+\-*&%=<>!?|\.@]/;

    // Tokenizer

    // Used as scratch variable to communicate multiple values without
    // consing up tons of objects.
    var tcat, content;
    function r(tc, style) {
        tcat = tc;
        return style;
    }

    function tokenBase(stream, state) {
        var ch = stream.next();
        if (ch == '"') {
            state.tokenize = tokenString;
            return state.tokenize(stream, state);
        }
        if (ch == "'") {
            tcat = "atom";
            if (stream.eat("\\")) {
                if (stream.skipTo("'")) { stream.next(); return "string"; }
                else { return "error"; }
            } else {
                stream.next();
                return stream.eat("'") ? "string" : "error";
            }
        }
        if (ch == "/") {
            if (stream.eat("/")) { stream.skipToEnd(); return "comment"; }
            if (stream.eat("*")) {
                state.tokenize = tokenComment(1);
                return state.tokenize(stream, state);
            }
        }
        if (ch == "#") {
            if (stream.eat("[")) { tcat = "open-attr"; return null; }
            stream.eatWhile(/\w/);
            return r("macro", "meta");
        }
        if (ch == ":" && stream.match(":<")) {
            return r("op", null);
        }
        if (ch.match(/\d/) || (ch == "." && stream.eat(/\d/))) {
            var flp = false;
            if (!stream.match(/^x[\da-f]+/i) && !stream.match(/^b[01]+/)) {
                stream.eatWhile(/\d/);
                if (stream.eat(".")) { flp = true; stream.eatWhile(/\d/); }
                if (stream.match(/^e[+\-]?\d+/i)) { flp = true; }
            }
            if (flp) stream.match(/^f(?:32|64)/);
            else stream.match(/^[ui](?:8|16|32|64)/);
            return r("atom", "number");
        }
        if (ch.match(/[()\[\]{}:;,]/)) return r(ch, null);
        if (ch == "-" && stream.eat(">")) return r("->", null);
        if (ch.match(operatorChar)) {
            stream.eatWhile(operatorChar);
            return r("op", null);
        }
        stream.eatWhile(/\w/);
        content = stream.current();
        if (stream.match(/^::\w/)) {
            stream.backUp(1);
            return r("prefix", "variable-2");
        }
        if (state.keywords.propertyIsEnumerable(content))
            return r(state.keywords[content], "keyword");
        return r("name", "variable");
    }

    function tokenString(stream, state) {
        var ch, escaped = false;
        while (ch = stream.next()) {
            if (ch == '"' && !escaped) {
                state.tokenize = tokenBase;
                return r("atom", "string");
            }
            escaped = !escaped && ch == "\\";
        }
        // Hack to not confuse the parser when a string is split in
        // pieces.
        return r("op", "string");
    }

    function tokenComment(depth) {
        return function(stream, state) {
            var lastCh = null, ch;
            while (ch = stream.next()) {
                if (ch == "/" && lastCh == "*") {
                    if (depth == 1) {
                        state.tokenize = tokenBase;
                        break;
                    } else {
                        state.tokenize = tokenComment(depth - 1);
                        return state.tokenize(stream, state);
                    }
                }
                if (ch == "*" && lastCh == "/") {
                    state.tokenize = tokenComment(depth + 1);
                    return state.tokenize(stream, state);
                }
                lastCh = ch;
            }
            return "comment";
        };
    }

    // Parser

    var cx = {state: null, stream: null, marked: null, cc: null};
    function pass() {
        for (var i = arguments.length - 1; i >= 0; i--) cx.cc.push(arguments[i]);
    }
    function cont() {
        pass.apply(null, arguments);
        return true;
    }

    function pushlex(type, info) {
        var result = function() {
            var state = cx.state;
            state.lexical = {indented: state.indented, column: cx.stream.column(),
                type: type, prev: state.lexical, info: info};
        };
        result.lex = true;
        return result;
    }
    function poplex() {
        var state = cx.state;
        if (state.lexical.prev) {
            if (state.lexical.type == ")")
                state.indented = state.lexical.indented;
            state.lexical = state.lexical.prev;
        }
    }
    function typecx() { cx.state.keywords = typeKeywords; }
    function valcx() { cx.state.keywords = valKeywords; }
    poplex.lex = typecx.lex = valcx.lex = true;

    function commasep(comb, end) {
        function more(type) {
            if (type == ",") return cont(comb, more);
            if (type == end) return cont();
            return cont(more);
        }
        return function(type) {
            if (type == end) return cont();
            return pass(comb, more);
        };
    }

    function block(type) {
        if (type == "}") return cont();
        if (type == "let") return cont(pushlex("stat", "let"), letdef, poplex, block);
        if (type == "fn") return cont(pushlex("stat"), fndef, poplex, block);
        if (type == "type") return cont(pushlex("stat"), tydef, endstatement, poplex, block);
        if (type == "tag") return cont(pushlex("stat"), tagdef, poplex, block);
        if (type == "mod") return cont(pushlex("stat"), mod, poplex, block);
        if (type == "open-attr") return cont(pushlex("]"), commasep(expression, "]"), poplex);
        if (type == "ignore") return cont(block);
        return pass(pushlex("stat"), expression, poplex, endstatement, block);
    }
    function endstatement(type) {
        if (type == ";") return cont();
        return pass();
    }
    function expression(type) {
        if (type == "atom" || type == "name") return cont(maybeop);
        if (type == "{") return cont(pushlex("}"), exprbrace, poplex);
        if (type.match(/[\[\(]/)) return matchBrackets(type, expression);
        if (type.match(/[\]\)\};,]/)) return pass();
        if (type == "if-style") return cont(expression, expression);
        if (type == "else-style" || type == "op") return cont(expression);
        if (type == "for") return cont(pattern, maybetype, inop, expression, expression);
        if (type == "alt") return cont(expression, altbody);
        if (type == "fn") return cont(fndef);
        if (type == "macro") return cont(macro);
        return cont();
    }
    function maybeop(type) {
        if (content == ".") return cont(maybeprop);
        if (content == "::<"){return cont(typarams, maybeop);}
        if (type == "op" || content == ":") return cont(expression);
        if (type == "(" || type == "[") return matchBrackets(type, expression);
        return pass();
    }
    function maybeprop(type) {
        if (content.match(/^\w+$/)) {cx.marked = "variable"; return cont(maybeop);}
        return pass(expression);
    }
    function exprbrace(type) {
        if (type == "op") {
            if (content == "|") return cont(blockvars, poplex, pushlex("}", "block"), block);
            if (content == "||") return cont(poplex, pushlex("}", "block"), block);
        }
        if (content == "mutable" || (content.match(/^\w+$/) && cx.stream.peek() == ":"
                                     && !cx.stream.match("::", false))) return pass(recliteral);
        return pass(block);
    }
    function recliteral(type) {
        if (content == "mutable" || content == "with") {cx.marked = "keyword"; return cont(recliteral);}
        if (content.match(/^\w*$/)) {cx.marked = "variable"; return cont(recliteral);}
        if (type == ":") return cont(expression, recliteral);
        if (type == "}") return cont();
        return cont(recliteral);
    }
    function blockvars(type) {
        if (type == "name") {cx.marked = "def"; return cont(blockvars);}
        if (type == "op" && content == "|") return cont();
        return cont(blockvars);
    }

    function letdef(type) {
        if (type == ";") return cont();
        if (content == "=") return cont(expression, letdef);
        if (type == ",") return cont(letdef);
        return pass(pattern, maybetype, letdef);
    }
    function maybetype(type) {
        if (type == ":") return cont(typecx, rtype, valcx);
        return pass();
    }
    function inop(type) {
        if (type == "name" && content == "in") {cx.marked = "keyword"; return cont();}
        return pass();
    }
    function fndef(type) {
        if (type == "name") {cx.marked = "def"; return cont(fndef);}
        if (content == "<") return cont(typarams, fndef);
        if (type == "{") return pass(expression);
        if (type == "(") return cont(pushlex(")"), commasep(argdef, ")"), poplex, fndef);
        if (type == "->") return cont(typecx, rtype, valcx, fndef);
        return cont(fndef);
    }
    function tydef(type) {
        if (type == "name") {cx.marked = "def"; return cont(tydef);}
        if (content == "<") return cont(typarams, tydef);
        if (content == "=") return cont(typecx, rtype, valcx);
        return cont(tydef);
    }
    function tagdef(type) {
        if (type == "name") {cx.marked = "def"; return cont(tagdef);}
        if (content == "<") return cont(typarams, tagdef);
        if (content == "=") return cont(typecx, rtype, valcx, endstatement);
        if (type == "{") return cont(pushlex("}"), typecx, tagblock, valcx, poplex);
        return cont(tagdef);
    }
    function tagblock(type) {
        if (type == "}") return cont();
        if (type == "(") return cont(pushlex(")"), commasep(rtype, ")"), poplex, tagblock);
        if (content.match(/^\w+$/)) cx.marked = "def";
        return cont(tagblock);
    }
    function mod(type) {
        if (type == "name") {cx.marked = "def"; return cont(mod);}
        if (type == "{") return cont(pushlex("}"), block, poplex);
        return pass();
    }
    function typarams(type) {
        if (content == ">") return cont();
        if (content == ",") return cont(typarams);
        return pass(rtype, typarams);
    }
    function argdef(type) {
        if (type == "name") {cx.marked = "def"; return cont(argdef);}
        if (type == ":") return cont(typecx, rtype, valcx);
        return pass();
    }
    function rtype(type) {
        if (type == "name") {cx.marked = "variable-3"; return cont(rtypemaybeparam); }
        if (content == "mutable") {cx.marked = "keyword"; return cont(rtype);}
        if (type == "atom") return cont(rtypemaybeparam);
        if (type == "op" || type == "obj") return cont(rtype);
        if (type == "fn") return cont(fntype);
        return matchBrackets(type, rtype);
    }
    function rtypemaybeparam(type) {
        if (content == "<") return cont(typarams);
        return pass();
    }
    function fntype(type) {
        if (type == "(") return cont(pushlex("("), commasep(rtype, ")"), poplex, fntype);
        if (type == "->") return cont(rtype);
        return pass();
    }
    function pattern(type) {
        if (type == "name") {cx.marked = "def"; return cont(patternmaybeop);}
        if (type == "atom") return cont(patternmaybeop);
        if (type == "op") return cont(pattern);
        return matchBrackets(type, pattern);
    }
    function patternmaybeop(type) {
        if (type == "op" && content == ".") return cont();
        if (content == "to") {cx.marked = "keyword"; return cont(pattern);}
        else return pass();
    }
    function altbody(type) {
        if (type == "{") return cont(pushlex("}", "alt"), altblock, poplex);
        return pass();
    }
    function altblock(type) {
        if (type == "}") return cont();
        if (type == "|") return cont(altblock);
        if (content == "when") {cx.marked = "keyword"; return cont(expression, altblock);}
        if (type == "{") return cont(pushlex("}", "alt"), block, poplex, altblock);
        return pass(pattern, altblock);
    }
    function macro(type) {
        if (type.match(/[\[\(\{]/)) return matchBrackets(type, expression);
        return pass();
    }
    function matchBrackets(type, comb) {
        if (type == "[") return cont(pushlex("]"), commasep(comb, "]"), poplex);
        if (type == "(") return cont(pushlex(")"), commasep(comb, ")"), poplex);
        if (type == "{") return cont(pushlex("}"), commasep(comb, "}"), poplex);
        return cont();
    }

    function parse(state, stream, style) {
        var cc = state.cc;
        // Communicate our context to the combinators.
        // (Less wasteful than consing up a hundred closures on every call.)
        cx.state = state; cx.stream = stream; cx.marked = null, cx.cc = cc;

        while (true) {
            var combinator = cc.length ? cc.pop() : block;
            if (combinator(tcat)) {
                while(cc.length && cc[cc.length - 1].lex)
                    cc.pop()();
                return cx.marked || style;
            }
        }
    }

    return {
        startState: function() {
            return {
                tokenize: tokenBase,
                cc: [],
                lexical: {indented: -indentUnit, column: 0, type: "top", align: false},
                keywords: valKeywords,
                indented: 0
            };
        },

        token: function(stream, state) {
            if (stream.sol()) {
                if (!state.lexical.hasOwnProperty("align"))
                    state.lexical.align = false;
                state.indented = stream.indentation();
            }
            if (stream.eatSpace()) return null;
            tcat = content = null;
            var style = state.tokenize(stream, state);
            if (style == "comment") return style;
            if (!state.lexical.hasOwnProperty("align"))
                state.lexical.align = true;
            if (tcat == "prefix") return style;
            if (!content) content = stream.current();
            return parse(state, stream, style);
        },

        indent: function(state, textAfter) {
            if (state.tokenize != tokenBase) return 0;
            var firstChar = textAfter && textAfter.charAt(0), lexical = state.lexical,
                type = lexical.type, closing = firstChar == type;
            if (type == "stat") return lexical.indented + indentUnit;
            if (lexical.align) return lexical.column + (closing ? 0 : 1);
            return lexical.indented + (closing ? 0 : (lexical.info == "alt" ? altIndentUnit : indentUnit));
        },

        electricChars: "{}"
    };
});

CodeMirror.defineMIME("text/x-rustsrc", "rust");

CodeMirror.defineMode("ruby", function(config, parserConfig) {
    function wordObj(words) {
        var o = {};
        for (var i = 0, e = words.length; i < e; ++i) o[words[i]] = true;
        return o;
    }
    var keywords = wordObj([
      "alias", "and", "BEGIN", "begin", "break", "case", "class", "def", "defined?", "do", "else",
      "elsif", "END", "end", "ensure", "false", "for", "if", "in", "module", "next", "not", "or",
      "redo", "rescue", "retry", "return", "self", "super", "then", "true", "undef", "unless",
      "until", "when", "while", "yield", "nil", "raise", "throw", "catch", "fail", "loop", "callcc",
      "caller", "lambda", "proc", "public", "protected", "private", "require", "load",
      "require_relative", "extend", "autoload"
    ]);
    var indentWords = wordObj(["def", "class", "case", "for", "while", "do", "module", "then",
                               "unless", "catch", "loop", "proc"]);
    var dedentWords = wordObj(["end", "until"]);
    var matching = {"[": "]", "{": "}", "(": ")"};
    var curPunc;

    function chain(newtok, stream, state) {
        state.tokenize.push(newtok);
        return newtok(stream, state);
    }

    function tokenBase(stream, state) {
        curPunc = null;
        if (stream.sol() && stream.match("=begin") && stream.eol()) {
            state.tokenize.push(readBlockComment);
            return "comment";
        }
        if (stream.eatSpace()) return null;
        var ch = stream.next();
        if (ch == "`" || ch == "'" || ch == '"' || ch == "/") {
            return chain(readQuoted(ch, "string", ch == '"'), stream, state);
        } else if (ch == "%") {
            var style, embed = false;
            if (stream.eat("s")) style = "atom";
            else if (stream.eat(/[WQ]/)) { style = "string"; embed = true; }
            else if (stream.eat(/[wxqr]/)) style = "string";
            var delim = stream.eat(/[^\w\s]/);
            if (!delim) return "operator";
            if (matching.propertyIsEnumerable(delim)) delim = matching[delim];
            return chain(readQuoted(delim, style, embed, true), stream, state);
        } else if (ch == "#") {
            stream.skipToEnd();
            return "comment";
        } else if (ch == "<" && stream.eat("<")) {
            stream.eat("-");
            stream.eat(/[\'\"\`]/);
            var match = stream.match(/^\w+/);
            stream.eat(/[\'\"\`]/);
            if (match) return chain(readHereDoc(match[0]), stream, state);
            return null;
        } else if (ch == "0") {
            if (stream.eat("x")) stream.eatWhile(/[\da-fA-F]/);
            else if (stream.eat("b")) stream.eatWhile(/[01]/);
            else stream.eatWhile(/[0-7]/);
            return "number";
        } else if (/\d/.test(ch)) {
            stream.match(/^[\d_]*(?:\.[\d_]+)?(?:[eE][+\-]?[\d_]+)?/);
            return "number";
        } else if (ch == "?") {
            while (stream.match(/^\\[CM]-/)) {}
            if (stream.eat("\\")) stream.eatWhile(/\w/);
            else stream.next();
            return "string";
        } else if (ch == ":") {
            if (stream.eat("'")) return chain(readQuoted("'", "atom", false), stream, state);
            if (stream.eat('"')) return chain(readQuoted('"', "atom", true), stream, state);
            stream.eatWhile(/[\w\?]/);
            return "atom";
        } else if (ch == "@") {
            stream.eat("@");
            stream.eatWhile(/[\w\?]/);
            return "variable-2";
        } else if (ch == "$") {
            stream.next();
            stream.eatWhile(/[\w\?]/);
            return "variable-3";
        } else if (/\w/.test(ch)) {
            stream.eatWhile(/[\w\?]/);
            if (stream.eat(":")) return "atom";
            return "ident";
        } else if (ch == "|" && (state.varList || state.lastTok == "{" || state.lastTok == "do")) {
            curPunc = "|";
            return null;
        } else if (/[\(\)\[\]{}\\;]/.test(ch)) {
            curPunc = ch;
            return null;
        } else if (ch == "-" && stream.eat(">")) {
            return "arrow";
        } else if (/[=+\-\/*:\.^%<>~|]/.test(ch)) {
            stream.eatWhile(/[=+\-\/*:\.^%<>~|]/);
            return "operator";
        } else {
            return null;
        }
    }

    function tokenBaseUntilBrace() {
        var depth = 1;
        return function(stream, state) {
            if (stream.peek() == "}") {
                depth--;
                if (depth == 0) {
                    state.tokenize.pop();
                    return state.tokenize[state.tokenize.length-1](stream, state);
                }
            } else if (stream.peek() == "{") {
                depth++;
            }
            return tokenBase(stream, state);
        };
    }
    function readQuoted(quote, style, embed, unescaped) {
        return function(stream, state) {
            var escaped = false, ch;
            while ((ch = stream.next()) != null) {
                if (ch == quote && (unescaped || !escaped)) {
                    state.tokenize.pop();
                    break;
                }
                if (embed && ch == "#" && !escaped && stream.eat("{")) {
                    state.tokenize.push(tokenBaseUntilBrace(arguments.callee));
                    break;
                }
                escaped = !escaped && ch == "\\";
            }
            return style;
        };
    }
    function readHereDoc(phrase) {
        return function(stream, state) {
            if (stream.match(phrase)) state.tokenize.pop();
            else stream.skipToEnd();
            return "string";
        };
    }
    function readBlockComment(stream, state) {
        if (stream.sol() && stream.match("=end") && stream.eol())
            state.tokenize.pop();
        stream.skipToEnd();
        return "comment";
    }

    return {
        startState: function() {
            return {tokenize: [tokenBase],
                indented: 0,
                context: {type: "top", indented: -config.indentUnit},
                continuedLine: false,
                lastTok: null,
                varList: false};
        },

        token: function(stream, state) {
            if (stream.sol()) state.indented = stream.indentation();
            var style = state.tokenize[state.tokenize.length-1](stream, state), kwtype;
            if (style == "ident") {
                var word = stream.current();
                style = keywords.propertyIsEnumerable(stream.current()) ? "keyword"
                  : /^[A-Z]/.test(word) ? "tag"
                  : (state.lastTok == "def" || state.lastTok == "class" || state.varList) ? "def"
                  : "variable";
                if (indentWords.propertyIsEnumerable(word)) kwtype = "indent";
                else if (dedentWords.propertyIsEnumerable(word)) kwtype = "dedent";
                else if (word == "if" && stream.column() == stream.indentation()) kwtype = "indent";
            }
            if (curPunc || (style && style != "comment")) state.lastTok = word || curPunc || style;
            if (curPunc == "|") state.varList = !state.varList;

            if (kwtype == "indent" || /[\(\[\{]/.test(curPunc))
                state.context = {prev: state.context, type: curPunc || style, indented: state.indented};
            else if ((kwtype == "dedent" || /[\)\]\}]/.test(curPunc)) && state.context.prev)
                state.context = state.context.prev;

            if (stream.eol())
                state.continuedLine = (curPunc == "\\" || style == "operator");
            return style;
        },

        indent: function(state, textAfter) {
            if (state.tokenize[state.tokenize.length-1] != tokenBase) return 0;
            var firstChar = textAfter && textAfter.charAt(0);
            var ct = state.context;
            var closing = ct.type == matching[firstChar] ||
              ct.type == "keyword" && /^(?:end|until|else|elsif|when)\b/.test(textAfter);
            return ct.indented + (closing ? 0 : config.indentUnit) +
              (state.continuedLine ? config.indentUnit : 0);
        }
    };
});

CodeMirror.defineMIME("text/x-ruby", "ruby");

CodeMirror.defineMode('rst', function(config, options) {
    function setState(state, fn, ctx) {
        state.fn = fn;
        setCtx(state, ctx);
    }

    function setCtx(state, ctx) {
        state.ctx = ctx || {};
    }

    function setNormal(state, ch) {
        if (ch && (typeof ch !== 'string')) {
            var str = ch.current();
            ch = str[str.length-1];
        }

        setState(state, normal, {back: ch});
    }

    function hasMode(mode) {
        if (mode) {
            var modes = CodeMirror.listModes();

            for (var i in modes) {
                if (modes[i] == mode) {
                    return true;
                }
            }
        }

        return false;
    }

    function getMode(mode) {
        if (hasMode(mode)) {
            return CodeMirror.getMode(config, mode);
        } else {
            return null;
        }
    }

    var verbatimMode = getMode(options.verbatim);
    var pythonMode = getMode('python');

    var reSection = /^[!"#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]/;
    var reDirective = /^\s*\w([-:.\w]*\w)?::(\s|$)/;
    var reHyperlink = /^\s*_[\w-]+:(\s|$)/;
    var reFootnote = /^\s*\[(\d+|#)\](\s|$)/;
    var reCitation = /^\s*\[[A-Za-z][\w-]*\](\s|$)/;
    var reFootnoteRef = /^\[(\d+|#)\]_/;
    var reCitationRef = /^\[[A-Za-z][\w-]*\]_/;
    var reDirectiveMarker = /^\.\.(\s|$)/;
    var reVerbatimMarker = /^::\s*$/;
    var rePreInline = /^[-\s"([{</:]/;
    var rePostInline = /^[-\s`'")\]}>/:.,;!?\\_]/;
    var reEnumeratedList = /^\s*((\d+|[A-Za-z#])[.)]|\((\d+|[A-Z-a-z#])\))\s/;
    var reBulletedList = /^\s*[-\+\*]\s/;
    var reExamples = /^\s+(>>>|In \[\d+\]:)\s/;

    function normal(stream, state) {
        var ch, sol, i;

        if (stream.eat(/\\/)) {
            ch = stream.next();
            setNormal(state, ch);
            return null;
        }

        sol = stream.sol();

        if (sol && (ch = stream.eat(reSection))) {
            for (i = 0; stream.eat(ch); i++);

            if (i >= 3 && stream.match(/^\s*$/)) {
                setNormal(state, null);
                return 'section';
            } else {
                stream.backUp(i + 1);
            }
        }

        if (sol && stream.match(reDirectiveMarker)) {
            if (!stream.eol()) {
                setState(state, directive);
            }

            return 'directive-marker';
        }

        if (stream.match(reVerbatimMarker)) {
            if (!verbatimMode) {
                setState(state, verbatim);
            } else {
                var mode = verbatimMode;

                setState(state, verbatim, {
                    mode: mode,
                    local: mode.startState()
                });
            }

            return 'verbatim-marker';
        }

        if (sol && stream.match(reExamples, false)) {
            if (!pythonMode) {
                setState(state, verbatim);
                return 'verbatim-marker';
            } else {
                var mode = pythonMode;

                setState(state, verbatim, {
                    mode: mode,
                    local: mode.startState()
                });

                return null;
            }
        }

        if (sol && (stream.match(reEnumeratedList) ||
                    stream.match(reBulletedList))) {
            setNormal(state, stream);
            return 'list';
        }

        function testBackward(re) {
            return sol || !state.ctx.back || re.test(state.ctx.back);
        }

        function testForward(re) {
            return stream.eol() || stream.match(re, false);
        }

        function testInline(re) {
            return stream.match(re) && testBackward(/\W/) && testForward(/\W/);
        }

        if (testInline(reFootnoteRef)) {
            setNormal(state, stream);
            return 'footnote';
        }

        if (testInline(reCitationRef)) {
            setNormal(state, stream);
            return 'citation';
        }

        ch = stream.next();

        if (testBackward(rePreInline)) {
            if ((ch === ':' || ch === '|') && stream.eat(/\S/)) {
                var token;

                if (ch === ':') {
                    token = 'role';
                } else {
                    token = 'replacement';
                }

                setState(state, inline, {
                    ch: ch,
                    wide: false,
                    prev: null,
                    token: token
                });

                return token;
            }

            if (ch === '*' || ch === '`') {
                var orig = ch,
                    wide = false;

                ch = stream.next();

                if (ch == orig) {
                    wide = true;
                    ch = stream.next();
                }

                if (ch && !/\s/.test(ch)) {
                    var token;

                    if (orig === '*') {
                        token = wide ? 'strong' : 'emphasis';
                    } else {
                        token = wide ? 'inline' : 'interpreted';
                    }

                    setState(state, inline, {
                        ch: orig,               // inline() has to know what to search for
                        wide: wide,             // are we looking for `ch` or `chch`
                        prev: null,             // terminator must not be preceeded with whitespace
                        token: token            // I don't want to recompute this all the time
                    });

                    return token;
                }
            }
        }

        setNormal(state, ch);
        return null;
    }

    function inline(stream, state) {
        var ch = stream.next(),
            token = state.ctx.token;

        function finish(ch) {
            state.ctx.prev = ch;
            return token;
        }

        if (ch != state.ctx.ch) {
            return finish(ch);
        }

        if (/\s/.test(state.ctx.prev)) {
            return finish(ch);
        }

        if (state.ctx.wide) {
            ch = stream.next();

            if (ch != state.ctx.ch) {
                return finish(ch);
            }
        }

        if (!stream.eol() && !rePostInline.test(stream.peek())) {
            if (state.ctx.wide) {
                stream.backUp(1);
            }

            return finish(ch);
        }

        setState(state, normal);
        setNormal(state, ch);

        return token;
    }

    function directive(stream, state) {
        var token = null;

        if (stream.match(reDirective)) {
            token = 'directive';
        } else if (stream.match(reHyperlink)) {
            token = 'hyperlink';
        } else if (stream.match(reFootnote)) {
            token = 'footnote';
        } else if (stream.match(reCitation)) {
            token = 'citation';
        } else {
            stream.eatSpace();

            if (stream.eol()) {
                setNormal(state, stream);
                return null;
            } else {
                stream.skipToEnd();
                setState(state, comment);
                return 'comment';
            }
        }

        setState(state, body, {start: true});
        return token;
    }

    function body(stream, state) {
        var token = 'body';

        if (!state.ctx.start || stream.sol()) {
            return block(stream, state, token);
        }

        stream.skipToEnd();
        setCtx(state);

        return token;
    }

    function comment(stream, state) {
        return block(stream, state, 'comment');
    }

    function verbatim(stream, state) {
        if (!verbatimMode) {
            return block(stream, state, 'verbatim');
        } else {
            if (stream.sol()) {
                if (!stream.eatSpace()) {
                    setNormal(state, stream);
                }

                return null;
            }

            return verbatimMode.token(stream, state.ctx.local);
        }
    }

    function block(stream, state, token) {
        if (stream.eol() || stream.eatSpace()) {
            stream.skipToEnd();
            return token;
        } else {
            setNormal(state, stream);
            return null;
        }
    }

    return {
        startState: function() {
            return {fn: normal, ctx: {}};
        },

        copyState: function(state) {
            return {fn: state.fn, ctx: state.ctx};
        },

        token: function(stream, state) {
            var token = state.fn(stream, state);
            return token;
        }
    };
});

CodeMirror.defineMIME("text/x-rst", "rst");









