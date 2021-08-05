#!/usr/bin/env python
# -*- coding: utf-8 -*-

from typing import List, Dict
from structure import Value
from parameter import DataSetting


class Customize:
    def __init__(self, parent, **kwargs):
        self._offset = 0x0
        self._parent = parent
        self.structure: Dict[str, Value] = {key: Value(self, *setting) for key, setting in kwargs.items()}
        # noinspection PyUnresolvedReferences
        self.setMinimumHeight(30)

    def sequence(self, name: str) -> List[int]:
        return [self.structure[name].get(self.offset + DataSetting.STEP * idx) for idx in range(DataSetting.COUNT)]

    def set_parent(self, parent):
        self._parent = parent

    @property
    def offset(self) -> int:
        if isinstance(self._parent, Customize):
            return self._offset + self._parent.offset
        return self._offset

    @offset.setter
    def offset(self, offset: int):
        self._offset = offset
        self.refresh()

    def refresh(self):
        pass

    def rewrite(self):
        pass
