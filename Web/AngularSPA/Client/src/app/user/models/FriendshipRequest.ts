import { User, userSchema } from "./User";
import { schema } from "normalizr";

export const friendshipRequestSchema = new schema.Entity('friendshipRequests', {
    requester: userSchema,
    requestee: userSchema
});

export const friendshipRequestListSchema = new schema.Array(friendshipRequestSchema);

export type FriendshipRequestStatus = "accepted" | "rejected";

export interface FriendshipRequest {
    id: string;
    requester: User;
    requestee: User;
    createdAt: Date;
    modifiedAt: Date;
    outcome: string;
}