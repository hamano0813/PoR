#!/usr/bin/env python
# -*- coding: utf-8 -*-

from typing import List
from PySide6.QtWidgets import QFrame
from PySide6.QtCore import Qt
from .customize import Customize


class BackgroundFrame(Customize, QFrame):
    def __init__(self, parent=None):
        QFrame.__init__(self, parent, Qt.Widget)
        Customize.__init__(self, parent)
        self._child: List[Customize] = list()
        self._editors = {}
        if isinstance(parent, BackgroundFrame):
            parent.add_child(self)

    def add_child(self, widget: Customize):
        self._child.append(widget)

    def refresh(self):
        for child in self._child:
            child.refresh()

    def __getitem__(self, name: str):
        return self._editors.get(name, None)

    def __setitem__(self, name: str, editor: Customize):
        editor.set_parent(self)
        self._editors[name] = editor
        self.add_child(editor)
