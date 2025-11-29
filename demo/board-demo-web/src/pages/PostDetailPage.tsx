import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { postsApi, commentsApi } from '../api';
import { Post, Comment } from '../types';
import { useAuth } from '../contexts/AuthContext';

export function PostDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  
  const [post, setPost] = useState<Post | null>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [newComment, setNewComment] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (id) {
      loadPost();
      loadComments();
    }
  }, [id]);

  const loadPost = async () => {
    setIsLoading(true);
    try {
      const data = await postsApi.getById(Number(id));
      setPost(data);
    } catch (err) {
      setError('게시물을 불러오는데 실패했습니다.');
    } finally {
      setIsLoading(false);
    }
  };

  const loadComments = async () => {
    try {
      const data = await commentsApi.getByPostId(Number(id));
      setComments(data);
    } catch (err) {
      console.error('Failed to load comments:', err);
    }
  };

  const handleLike = async () => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    try {
      const result = await postsApi.toggleLike(Number(id));
      setPost(prev => prev ? { ...prev, likeCount: result.likeCount } : null);
    } catch (err) {
      console.error('Failed to toggle like:', err);
    }
  };

  const handleCommentSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newComment.trim() || !isAuthenticated) return;

    setIsSubmitting(true);
    try {
      await commentsApi.create(Number(id), { content: newComment });
      setNewComment('');
      loadComments();
      setPost(prev => prev ? { ...prev, commentCount: prev.commentCount + 1 } : null);
    } catch (err) {
      console.error('Failed to create comment:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeletePost = async () => {
    if (!confirm('정말 삭제하시겠습니까?')) return;
    
    try {
      await postsApi.delete(Number(id));
      navigate('/posts');
    } catch (err) {
      console.error('Failed to delete post:', err);
    }
  };

  const handleDeleteComment = async (commentId: number) => {
    if (!confirm('댓글을 삭제하시겠습니까?')) return;
    
    try {
      await commentsApi.delete(commentId);
      loadComments();
      setPost(prev => prev ? { ...prev, commentCount: prev.commentCount - 1 } : null);
    } catch (err) {
      console.error('Failed to delete comment:', err);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('ko-KR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (isLoading) {
    return (
      <div className="text-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
      </div>
    );
  }

  if (error || !post) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600 mb-4">{error || '게시물을 찾을 수 없습니다.'}</p>
        <Link to="/posts" className="text-blue-600 hover:underline">
          목록으로 돌아가기
        </Link>
      </div>
    );
  }

  const canEdit = user?.id === post.authorId || user?.role === 'Admin';

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* 게시물 내용 */}
      <article className="bg-white rounded-lg shadow-sm p-6">
        <div className="mb-4">
          <div className="flex items-center gap-2 mb-2">
            {post.isPinned && (
              <span className="px-2 py-0.5 bg-red-100 text-red-600 text-xs font-medium rounded">
                📌 공지
              </span>
            )}
            {post.category && (
              <span className="px-2 py-0.5 bg-gray-100 text-gray-600 text-xs font-medium rounded">
                {post.category}
              </span>
            )}
          </div>
          <h1 className="text-2xl font-bold text-gray-900">{post.title}</h1>
        </div>

        <div className="flex items-center justify-between pb-4 border-b">
          <div className="flex items-center gap-4 text-sm text-gray-500">
            <span className="font-medium text-gray-700">{post.authorName}</span>
            <span>{formatDate(post.createdAt)}</span>
            <span>조회 {post.viewCount}</span>
          </div>
          {canEdit && (
            <div className="flex gap-2">
              <Link
                to={`/posts/${post.id}/edit`}
                className="px-3 py-1 text-sm text-gray-600 hover:text-blue-600"
              >
                수정
              </Link>
              <button
                onClick={handleDeletePost}
                className="px-3 py-1 text-sm text-red-600 hover:text-red-700"
              >
                삭제
              </button>
            </div>
          )}
        </div>

        <div className="py-6 prose max-w-none">
          <div className="whitespace-pre-wrap">{post.content}</div>
        </div>

        {post.tags && post.tags.length > 0 && (
          <div className="flex gap-2 flex-wrap pt-4 border-t">
            {post.tags.map((tag, i) => (
              <span
                key={i}
                className="px-2 py-1 bg-blue-50 text-blue-600 text-sm rounded"
              >
                #{tag}
              </span>
            ))}
          </div>
        )}

        <div className="flex items-center gap-4 pt-4 mt-4 border-t">
          <button
            onClick={handleLike}
            className="flex items-center gap-1 px-4 py-2 rounded-lg border hover:bg-gray-50 transition"
          >
            <span>❤️</span>
            <span>{post.likeCount}</span>
          </button>
        </div>
      </article>

      {/* 댓글 섹션 */}
      <section className="bg-white rounded-lg shadow-sm p-6">
        <h2 className="text-lg font-semibold mb-4">댓글 {post.commentCount}개</h2>

        {/* 댓글 작성 */}
        {isAuthenticated ? (
          <form onSubmit={handleCommentSubmit} className="mb-6">
            <textarea
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              placeholder="댓글을 입력하세요..."
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
              rows={3}
            />
            <div className="flex justify-end mt-2">
              <button
                type="submit"
                disabled={!newComment.trim() || isSubmitting}
                className="px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition disabled:opacity-50"
              >
                {isSubmitting ? '등록 중...' : '댓글 등록'}
              </button>
            </div>
          </form>
        ) : (
          <div className="mb-6 p-4 bg-gray-50 rounded-lg text-center">
            <Link to="/login" className="text-blue-600 hover:underline">
              로그인
            </Link>
            하여 댓글을 작성하세요.
          </div>
        )}

        {/* 댓글 목록 */}
        <div className="space-y-4">
          {comments.length === 0 ? (
            <p className="text-center text-gray-500 py-4">
              첫 번째 댓글을 작성해보세요!
            </p>
          ) : (
            comments.map((comment) => (
              <div
                key={comment.id}
                className={`p-4 rounded-lg ${
                  comment.parentId ? 'ml-8 bg-gray-50' : 'bg-white border'
                }`}
              >
                <div className="flex items-center justify-between mb-2">
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-gray-800">
                      {comment.authorName}
                    </span>
                    <span className="text-sm text-gray-500">
                      {formatDate(comment.createdAt)}
                    </span>
                  </div>
                  {(user?.id === comment.authorId || user?.role === 'Admin') && (
                    <button
                      onClick={() => handleDeleteComment(comment.id)}
                      className="text-sm text-red-600 hover:underline"
                    >
                      삭제
                    </button>
                  )}
                </div>
                <p className="text-gray-700 whitespace-pre-wrap">{comment.content}</p>
              </div>
            ))
          )}
        </div>
      </section>

      {/* 목록 버튼 */}
      <div className="text-center">
        <Link
          to="/posts"
          className="inline-block px-6 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition"
        >
          목록으로
        </Link>
      </div>
    </div>
  );
}
