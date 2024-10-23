interface Item {
  id: number;
  by: string;
  type: "chime" | "comment";
  text: string;
  byId: number;
  time: number;
  kids: number[];
  deleted: boolean;
  parentId: null | number;
  mediaUrl: null | string;
}

export interface Chime extends Item {
  type: "chime";
}

export interface Comment extends Item {
  type: "comment";
}
