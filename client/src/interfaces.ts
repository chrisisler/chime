export interface Item {
  id: number;
  by: string;
  type: 'chime' | 'comment';
  text: string;
  byId: number;
  time: number;
  kids: number[];
  deleted: boolean;
  parentId: null | number;
  mediaUrl: null | string;
}

export const Item = {
  isChime(x: Item): x is Chime {
    return x.type === 'chime';
  },
  isComment(x: Item): x is Comment {
    return x.type === 'comment';
  }
};

export interface Chime extends Item {
  type: 'chime';
  parentId: null;
}

export interface Comment extends Item {
  type: 'comment';
  parentId: number;
}
