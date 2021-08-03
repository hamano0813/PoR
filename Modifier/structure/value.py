#!/usr/bin/env python
# -*- coding: utf-8 -*-

from dolphin_memory_engine import read_bytes, write_bytes


class Value:
    def __init__(self, parent, address: int, length: int, bit=None):
        self.parent = parent
        self.address = address
        self.length = length
        self.bit = bit

    def get(self, offset: int = 0x0) -> int:
        address = self.address + offset
        value = int.from_bytes(read_bytes(address, abs(self.length)), 'big', signed=(self.length < 0))
        value = self._load_bit(value) if self.bit is not None else value
        return value

    def set(self, value: int, offset: int = 0x0):
        address = self.address + offset
        if self.bit is not None:
            o_value = int.from_bytes(read_bytes(address, abs(self.length)), 'big', signed=(self.length < 0))
            _value = self._save_bit(o_value, value)
        else:
            _value = value
        data = _value.to_bytes(abs(self.length), 'big', signed=(self.length < 0))
        write_bytes(address, data)

    def _load_bit(self, value: int) -> int:
        if isinstance(self.bit, int):
            return (value & (1 << self.bit)) >> self.bit
        return (value & ((1 << self.bit[1]) - 1)) >> self.bit[0]

    def _save_bit(self, o_value: int, value: int) -> int:
        if isinstance(self.bit, int):
            return o_value | (1 << self.bit) if value else o_value & ~ (1 << self.bit)
        return ((o_value >> self.bit[1]) << self.bit[1]) | (o_value & ((1 << self.bit[0]) - 1)) | (value << self.bit[0])
