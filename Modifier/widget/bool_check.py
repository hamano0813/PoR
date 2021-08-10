#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QCheckBox
from PySide6.QtCore import Qt, QThread
from .customize import Customize


class LockThread(QThread):
    def __init__(self, structure, offset: int, lock_value: int):
        super(LockThread, self).__init__()
        self.structure = structure
        self.offset = offset
        self.lock_value = lock_value

    def run(self):
        while True:
            self.structure.set(self.lock_value, self.offset)
            self.msleep(100)


class BoolCheck(Customize, QCheckBox):
    thread_pool = dict()

    def __init__(self, parent, text, **kwargs):
        QCheckBox.__init__(self, parent)
        Customize.__init__(self, parent, **kwargs)
        self._text: dict[int, tuple[str, int]] = dict()

        if isinstance(text, str):
            self.setText(text)
        elif isinstance(text, dict):
            self.setTristate(True)
            for idx, (t, v) in enumerate(text.items()):
                self._text[idx] = (t, v)

    # noinspection PyUnresolvedReferences,PyTypeChecker
    def refresh(self):
        self.disconnect(self)
        if not self._text:
            self.setChecked(self.structure['value'].get(self.offset))
        else:
            offset = self.offset
            if self.thread_pool.get(offset):
                self.setCheckState(Qt.PartiallyChecked)
            elif self.structure['value'].get(self.offset):
                self.setCheckState(Qt.Checked)
            else:
                self.setCheckState(Qt.Unchecked)
            self.setText(self._text[self.checkState()][0])
        self.stateChanged.connect(self.rewrite)

    # noinspection PyUnresolvedReferences,PyTypeChecker
    def rewrite(self):
        if not self._text:
            self.structure['value'].set(self.isChecked(), self.offset)
        else:
            offset = self.offset
            text, value = self._text[self.checkState()]
            if self.checkState() == Qt.PartiallyChecked:
                lock = LockThread(self.structure['value'], offset, value)
                self.thread_pool[self.offset] = lock
                lock.start()
            else:
                if lock := self.thread_pool.pop(offset, None):
                    lock.terminate()
                    del lock
                self.structure['value'].set(value, offset)
            self.setText(text)
