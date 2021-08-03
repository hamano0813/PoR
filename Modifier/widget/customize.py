#!/usr/bin/env python
# -*- coding: utf-8 -*-

from typing import List, Dict
from structure import Value
from parameter import DataSetting


class Customize:
    def __init__(self, parent, **kwargs):
        self.__offset = 0x0
        self.__parent = parent
        self.structure: Dict[str, Value] = {key: Value(self, *setting) for key, setting in kwargs.items()}
        # noinspection PyUnresolvedReferences
        self.setMinimumHeight(25)

    def sequence(self, name: str) -> List[int]:
        return [self.structure[name].get(self.offset + DataSetting.STEP * idx) for idx in range(DataSetting.COUNT)]

    def set_parent(self, parent):
        self.__parent = parent

    @property
    def offset(self) -> int:
        if isinstance(self.__parent, Customize):
            return self.__offset + self.__parent.offset
        return self.__offset

    @offset.setter
    def offset(self, offset: int):
        self.__offset = offset
        self.refresh()

    def refresh(self):
        pass

    def rewrite(self):
        pass
